using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.LogicSystems;
using GameFramework.SituationSystems;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure.ModelViewer;
using SampleGame.Presentation;
using SampleGame.Presentation.ModelViewer;
using UniRx;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class ModelViewerSceneSituation : SceneSituation {
        /// <summary>
        /// Body生成用のBuilder
        /// </summary>
        private class BodyBuilder : IBodyBuilder {
            public void Build(IBody body, GameObject gameObject) {
                if (gameObject.GetComponent<AvatarController>() == null) {
                    body.AddController(gameObject.AddComponent<AvatarController>());
                }
            }
        }

        private int _debugPageId;
        private ModelViewerConfigData _configData;
        private ModelViewerAppService _appService;
        private ModelViewerDomainService _domainService;

        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/Develop/model_viewer.unity";
        protected override string EmptySceneAssetPath => "Assets/SampleGame/Scenes/empty.unity";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);

            async UniTask LoadAsync(CancellationToken ct) {
                // Config読み込み
                var assetManager = Services.Get<AssetManager>();
                await new ModelViewerConfigDataRequest()
                    .LoadAsync(assetManager, scope, cancellationToken: ct)
                    .ContinueWith(x => {
                        _configData = x;
                        ServiceContainer.Set(x).ScopeTo(scope);
                    });
            }

            yield return LoadAsync(scope.Token).ToCoroutine();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            yield return SetupInfrastructureRoutine(scope);
            yield return SetupManagerRoutine(scope);
            yield return SetupDomainRoutine(scope);
            yield return SetupApplicationRoutine(scope);
            yield return SetupFactoryRoutine(scope);
            yield return SetupPresentationRoutine(scope);

            // カメラ操作用Controllerの設定
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.SetCameraController("Default", new PreviewCameraController(_configData.camera));

            // 初期値反映
            _appService.ChangePreviewActor(_domainService.ModelViewerModel.MasterData.DefaultActorAssetKeyIndex);
            _appService.ChangeEnvironment(_domainService.ModelViewerModel.MasterData.DefaultEnvironmentAssetKeyIndex);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var appService = Services.Get<ModelViewerAppService>();
            var viewerModel = appService.DomainService.ModelViewerModel;

            // DebugPage初期化
            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            var motionsPageId = -1;
            _debugPageId = rootPage.AddPageLinkButton("Model Viewer", onLoad: pageTuple => {
                // モーションページの初期化
                void SetupMotionPage(IPreviewActorMaster setupData) {
                    if (motionsPageId >= 0) {
                        pageTuple.page.RemoveItem(motionsPageId);
                        motionsPageId = -1;
                    }

                    if (setupData == null) {
                        return;
                    }

                    motionsPageId = pageTuple.page.AddPageLinkButton("Motions", onLoad: motionsPageTuple => {
                        var clips = setupData.AnimationClips;
                        for (var i = 0; i < clips.Count; i++) {
                            var index = i;
                            var clip = clips[i];
                            motionsPageTuple.page.AddButton(clip.name, clicked: () => { appService.ChangeAnimationClip(index); });
                        }
                    });
                }

                // Environment
                pageTuple.page.AddPageLinkButton("Environments", onLoad: fieldsPageTuple => {
                    var environmentAssetKeys = viewerModel.MasterData.EnvironmentAssetKeys;
                    for (var i = 0; i < environmentAssetKeys.Count; i++) {
                        var index = i;
                        fieldsPageTuple.page.AddButton(environmentAssetKeys[i],
                            clicked: () => appService.ChangeEnvironment(index));
                    }
                });

                // PreviewActor
                pageTuple.page.AddPageLinkButton("Models", onLoad: modelsPageTuple => {
                    var actorAssetKeys = viewerModel.MasterData.ActorAssetKeys;
                    for (var i = 0; i < actorAssetKeys.Count; i++) {
                        var index = i;
                        modelsPageTuple.page.AddButton(actorAssetKeys[i], clicked: () => { appService.ChangePreviewActor(index); });
                    }
                });

                // Actor生成監視
                viewerModel.CreatedPreviewActorSubject
                    .TakeUntil(scope)
                    .StartWith(() => new CreatedPreviewActorDto {
                        ActorModel = viewerModel.ActorModel
                    })
                    .Subscribe(dto => {
                        if (dto.ActorModel != null) {
                            SetupMotionPage(dto.ActorModel.Master);
                        }
                    });

                // 初期状態反映
                SetupMotionPage(_domainService.PreviewActorModel.Master);
            });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal(TransitionHandle handle) {
            // Debugページ削除
            var debugSheet = Services.Get<DebugSheet>();
            if (debugSheet != null) {
                var rootPage = debugSheet.GetOrCreateInitialPage();
                rootPage.RemoveItem(_debugPageId);
            }

            base.DeactivateInternal(handle);
        }

        /// <summary>
        /// Infrastructure層の初期化
        /// </summary>
        private IEnumerator SetupInfrastructureRoutine(IScope scope) {
            var assetManager = Services.Get<AssetManager>();
            var repository = new ModelViewerRepository(assetManager);
            ServiceContainer.Set<IModelViewerRepository>(repository);

            yield break;
        }

        /// <summary>
        /// Managerの初期化
        /// </summary>
        private IEnumerator SetupManagerRoutine(IScope scope) {
            var repository = Services.Get<IModelViewerRepository>();
            var bodyBuilder = new BodyBuilder();
            var bodyManager = new BodyManager(bodyBuilder);
            bodyManager.RegisterTask(TaskOrder.Body);
            ServiceContainer.Set(bodyManager);

            var environmentManager = new EnvironmentManager();
            ServiceContainer.Set(environmentManager);

            var actorManager = new ActorEntityManager(bodyManager, repository);
            ServiceContainer.Set(actorManager);

            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);

            yield break;
        }

        /// <summary>
        /// Domain層の初期化
        /// </summary>
        private IEnumerator SetupDomainRoutine(IScope scope) {
            // モデルの生成
            ModelViewerModel.Create().ScopeTo(scope);
            RecordingModel.Create().ScopeTo(scope);
            SettingsModel.Create().ScopeTo(scope);
            
            _domainService = new ModelViewerDomainService();
            ServiceContainer.Set(_domainService).ScopeTo(scope);

            yield break;
        }

        /// <summary>
        /// Application層の初期化
        /// </summary>
        private IEnumerator SetupApplicationRoutine(IScope scope) {
            _appService = new ModelViewerAppService();
            ServiceContainer.Set(_appService).ScopeTo(scope);

            yield return _appService.SetupAsync(_configData.master, scope.Token).ToCoroutine();
        }

        /// <summary>
        /// Factoryの初期化
        /// </summary>
        private IEnumerator SetupFactoryRoutine(IScope scope) {
            var actorFactory = new PreviewActorFactory();
            _appService.SetFactory(actorFactory);
            yield break;
        }

        /// <summary>
        /// Presentation層の初期化
        /// </summary>
        private IEnumerator SetupPresentationRoutine(IScope scope) {
            void SetupLogic(Logic logic, bool addService = false) {
                logic.Activate();
                logic.RegisterTask(TaskOrder.Logic);
                logic.ScopeTo(scope);

                if (addService) {
                    ServiceContainer.Set(logic);
                }
            }

            SetupLogic(new ModelViewerPresenter());

            yield break;
        }
    }
}