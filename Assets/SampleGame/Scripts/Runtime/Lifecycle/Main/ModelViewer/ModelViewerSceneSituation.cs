using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.ActorSystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using R3;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.ModelViewer;
using SampleGame.Presentation;
using SampleGame.Presentation.ModelViewer;
using ThirdPersonEngine;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class ModelViewerSceneSituation : SceneSituation {
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
                var assetManager = Services.Resolve<AssetManager>();
                await new ModelViewerConfigDataRequest()
                    .LoadAsync(assetManager, scope, cancellationToken: ct)
                    .ContinueWith(x => {
                        _configData = x;
                        ServiceContainer.RegisterInstance(x).RegisterTo(scope);
                    });
            }

            yield return LoadAsync(scope.Token).ToCoroutine();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            SetupInfrastructures(scope);
            SetupManagers(scope);
            SetupFactories(scope);
            SetupDomains(scope);
            SetupApplications(scope);
            SetupPresentations(scope);

            // アプリケーション初期化
            yield return _appService.SetupAsync(_configData.master, scope.Token).ToCoroutine();

            // カメラ操作用Controllerの設定
            var cameraManager = Services.Resolve<CameraManager>();
            cameraManager.SetCameraController("Default", new PreviewCameraController(_configData.camera));
            
            // Recorderのセットアップ
            var recorder = Services.Resolve<ModelRecorder>();
            recorder.ActorSlot = Services.Resolve<ActorEntityManager>().RootTransform;

            // 初期値反映
            _appService.ChangePreviewActor(_domainService.ModelViewerModel.Master.DefaultActorAssetKeyIndex);
            _appService.ChangeEnvironment(_domainService.ModelViewerModel.Master.DefaultEnvironmentAssetKeyIndex);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var appService = Services.Resolve<ModelViewerAppService>();
            var viewerModel = appService.DomainService.ModelViewerModel;

            // DebugPage初期化
            var debugSheet = Services.Resolve<DebugSheet>();
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
                    var environmentAssetKeys = viewerModel.Master.EnvironmentAssetKeys;
                    for (var i = 0; i < environmentAssetKeys.Count; i++) {
                        var index = i;
                        fieldsPageTuple.page.AddButton(environmentAssetKeys[i],
                            clicked: () => appService.ChangeEnvironment(index));
                    }
                });

                // PreviewActor
                pageTuple.page.AddPageLinkButton("Models", onLoad: modelsPageTuple => {
                    var actorAssetKeys = viewerModel.Master.ActorAssetKeys;
                    for (var i = 0; i < actorAssetKeys.Count; i++) {
                        var index = i;
                        modelsPageTuple.page.AddButton(actorAssetKeys[i], clicked: () => { appService.ChangePreviewActor(index); });
                    }
                });

                // Actor生成監視
                viewerModel.ChangedPreviewActorSubject
                    .TakeUntil(scope)
                    .Prepend(() => new ChangedPreviewActorDto {
                        Model = viewerModel.PreviewActorModel
                    })
                    .Subscribe(dto => {
                        if (dto.Model != null) {
                            SetupMotionPage(dto.Model.Master);
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
            var debugSheet = Services.Resolve<DebugSheet>();
            if (debugSheet != null) {
                var rootPage = debugSheet.GetOrCreateInitialPage();
                rootPage.RemoveItem(_debugPageId);
            }

            base.DeactivateInternal(handle);
        }

        /// <summary>
        /// Infrastructure層の初期化
        /// </summary>
        private void SetupInfrastructures(IScope scope) {
            ServiceContainer.Register<IModelRepository, ModelRepository>().RegisterTo(scope);
            ServiceContainer.Register<IModelViewerRepository, ModelViewerRepository>().RegisterTo(scope);
            ServiceContainer.Register<EnvironmentSceneRepository>().RegisterTo(scope);
        }

        /// <summary>
        /// Managerの初期化
        /// </summary>
        private void SetupManagers(IScope scope) {
            var actorManager = new ActorEntityManager();
            ServiceContainer.RegisterInstance(actorManager).RegisterTo(scope);

            var cameraManager = Services.Resolve<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
        }

        /// <summary>
        /// Domain層の初期化
        /// </summary>
        private void SetupDomains(IScope scope) {
            // モデルの生成
            var modelRepository = Services.Resolve<IModelRepository>();
            modelRepository.CreateSingleModel<ModelViewerModel>().RegisterTo(scope);
            modelRepository.CreateSingleModel<RecordingModel>().RegisterTo(scope);
            modelRepository.CreateSingleModel<SettingsModel>().RegisterTo(scope);
            
            _domainService = new ModelViewerDomainService();
            ServiceContainer.RegisterInstance(_domainService).RegisterTo(scope);
        }

        /// <summary>
        /// Application層の初期化
        /// </summary>
        private void SetupApplications(IScope scope) {
            _appService = new ModelViewerAppService();
            ServiceContainer.RegisterInstance(_appService).RegisterTo(scope);
        }

        /// <summary>
        /// Factoryの初期化
        /// </summary>
        private void SetupFactories(IScope scope) {
            ServiceContainer.Register<IPreviewActorFactory, PreviewActorFactory>().RegisterTo(scope);
            ServiceContainer.Register<IEnvironmentActorFactory, EnvironmentActorFactory>().RegisterTo(scope);
        }

        /// <summary>
        /// Presentation層の初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            void SetupLogic(Logic logic, bool addService = false) {
                logic.Activate();
                logic.RegisterTask(TaskOrder.Logic);
                logic.RegisterTo(scope);

                if (addService) {
                    ServiceContainer.RegisterInstance(logic).RegisterTo(scope);
                }
            }

            SetupLogic(new ModelViewerPresenter());
        }
    }
}