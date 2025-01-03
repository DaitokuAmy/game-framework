using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.LogicSystems;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure.ModelViewer;
using SampleGame.Presentation;
using SampleGame.Presentation.ModelViewer;
using UniRx;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEditor;
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
        private ModelViewerAppService _appService;
        private ModelViewerDomainService _domainService;

        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/Develop/model_viewer.unity";
        protected override string EmptySceneAssetPath => "Assets/SampleGame/Scenes/empty.unity";

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            yield return SetupDomainRoutine(scope);
            yield return SetupInfrastructureRoutine(scope);
            yield return SetupApplicationRoutine(scope);
            yield return SetupManagerRoutine(scope);
            yield return SetupPresentationRoutine(scope);

            var settings = Services.Get<ModelViewerSettings>();

            // カメラ操作用Controllerの設定
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.SetCameraController("Default", new PreviewCameraController(settings.Camera));

            // Editor用のSetupData自動更新処理
            UpdateModelViewerSetupData(_domainService.ModelViewerModel.Master);

            // 初期値反映
            _appService.ChangePreviewActor(_domainService.ModelViewerModel.Master.DefaultActorAssetKeyIndex);
            _appService.ChangeEnvironment(_domainService.ModelViewerModel.Master.DefaultEnvironmentAssetKeyIndex);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var ct = scope.Token;
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
                        for (var i = 0; i < clips.Length; i++) {
                            var index = i;
                            var clip = clips[i];
                            motionsPageTuple.page.AddButton(clip.name, clicked: () => { appService.ChangeAnimationClip(index); });
                        }
                    });
                }

                // Environment
                pageTuple.page.AddPageLinkButton("Environments", onLoad: fieldsPageTuple => {
                    var environmentAssetKeys = viewerModel.Master.EnvironmentAssetKeys;
                    for (var i = 0; i < environmentAssetKeys.Length; i++) {
                        var index = i;
                        fieldsPageTuple.page.AddButton(environmentAssetKeys[i],
                            clicked: () => appService.ChangeEnvironment(index));
                    }
                });

                // PreviewActor
                pageTuple.page.AddPageLinkButton("Models", onLoad: modelsPageTuple => {
                    var actorAssetKeys = viewerModel.Master.ActorAssetKeys;
                    for (var i = 0; i < actorAssetKeys.Length; i++) {
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
            _domainService = new ModelViewerDomainService();
            ServiceContainer.Set(_domainService).ScopeTo(scope);

            yield break;
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
        /// Application層の初期化
        /// </summary>
        private IEnumerator SetupApplicationRoutine(IScope scope) {
            _appService = new ModelViewerAppService();
            ServiceContainer.Set(_appService).ScopeTo(scope);

            yield return _appService.SetupAsync(scope.Token).ToCoroutine();
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

        /// <summary>
        /// エディタ実行時用のModelViewerSetupDataの更新処理
        /// </summary>
        private void UpdateModelViewerSetupData(IModelViewerMaster master) {
#if UNITY_EDITOR
            if (master is not ModelViewerSetupData modelViewerSetup) {
                return;
            }

            var ids = new HashSet<string>();
            var guids = AssetDatabase.FindAssets($"t:{nameof(PreviewActorSetupData)}");
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var assetKey = fileName.Replace("dat_preview_actor_setup_", "");
                ids.Add(assetKey);
            }

            modelViewerSetup.actorAssetKeys = ids.OrderBy(x => x).ToArray();
            EditorUtility.SetDirty(modelViewerSetup);
            AssetDatabase.Refresh();
#endif
        }
    }
}