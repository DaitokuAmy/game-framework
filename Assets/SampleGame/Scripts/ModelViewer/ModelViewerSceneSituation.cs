using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityDebugSheet.Runtime.Core.Scripts;
using SampleGame.ModelViewer;

namespace SampleGame {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class ModelViewerSceneSituation : SceneSituation {
        private int _debugPageId;
        
        protected override string SceneAssetPath => "model_viewer";

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            var ct = scope.Token;

            var settings = Services.Get<ModelViewerSettings>();
            
            // Modelの生成
            var model = ModelViewerModel.Create()
                .ScopeTo(scope);
            
            // Repositoryの生成
            var repository = new ModelViewerRepository(Services.Get<AssetManager>());
            repository.ScopeTo(scope);
            
            // ApplicationServiceの生成
            var appService = new ModelViewerApplicationService(model, repository);
            ServiceContainer.Set(appService);
            
            // ApplicationServiceの初期化
            yield return appService.SetupAsync(ct)
                .ToCoroutine();
            
            // 各種管理クラス生成/初期化
            var bodyManager = new BodyManager();
            bodyManager.RegisterTask(TaskOrder.Body);
            ServiceContainer.Set(bodyManager);

            var environmentManager = new EnvironmentManager();
            ServiceContainer.Set(environmentManager);

            var actorManager = new ActorManager(settings.PreviewSlot);
            ServiceContainer.Set(actorManager);
            
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);

            var recordingController = Services.Get<RecordingController>();
            recordingController.RegisterTask(TaskOrder.Logic);
            
            // カメラ操作用Controllerの設定
            cameraManager.SetCameraController("Default", new PreviewCameraController());

            // Editor用のSetupData自動更新処理
            UpdateModelViewerSetupData(model.SetupData);
                    
            // 初期状態反映
            appService.ChangeEnvironment(model.SetupData.defaultEnvironmentId);
            yield return appService.ChangePreviewActorAsync(model.SetupData.defaultActorDataId, ct)
                .ToCoroutine();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var ct = scope.Token;
            var model = ModelViewerModel.Get();
            var appService = Services.Get<ModelViewerApplicationService>();
            
            // Presenter初期化
            var presenter = new ModelViewerPresenter(model)
                .ScopeTo(scope);
            presenter.RegisterTask(TaskOrder.Logic);
            presenter.Activate();
            
            // DebugPage初期化
            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            var motionsPageId = -1;
            _debugPageId = rootPage.AddPageLinkButton("Model Viewer", onLoad: pageTuple => {
                // モーションページの初期化
                void SetupMotionPage(PreviewActorSetupData setupData) {
                    if (motionsPageId >= 0) {
                        pageTuple.page.RemoveItem(motionsPageId);
                        motionsPageId = -1;
                    }

                    if (setupData == null) {
                        return;
                    }
                        
                    motionsPageId = pageTuple.page.AddPageLinkButton("Motions", onLoad: motionsPageTuple => {
                        var clips = setupData.animationClips;
                        for (var i = 0; i < clips.Length; i++) {
                            var index = i;
                            var clip = clips[i];
                            motionsPageTuple.page.AddButton(clip.name, clicked: () => {
                                model.PreviewActorModel.ChangeClip(index);
                            });
                        }
                    });
                }
                
                // Environment
                pageTuple.page.AddPageLinkButton("Environments", onLoad: fieldsPageTuple => {
                    var environmentIds = model.SetupData.environmentIds;
                    foreach (var environmentId in environmentIds) {
                        var id = environmentId;
                        fieldsPageTuple.page.AddButton(environmentId,
                            clicked: () => appService.ChangeEnvironment(id));
                    }
                });
                
                // PreviewActor
                pageTuple.page.AddPageLinkButton("Models", onLoad: modelsPageTuple => {
                    var actorDataIds = model.SetupData.actorDataIds;
                    foreach (var actorDataId in actorDataIds) {
                        var id = actorDataId;
                        modelsPageTuple.page.AddButton(actorDataId, clicked:() => {
                            appService.ChangePreviewActorAsync(id, ct)
                                .ContinueWith(SetupMotionPage);
                        });
                    }
                });
                
                // 初期状態反映
                SetupMotionPage(model.PreviewActorModel.SetupData.Value);
            });
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal(TransitionHandle handle) {
            // Debugページ削除
            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            rootPage.RemoveItem(_debugPageId);
            
            base.DeactivateInternal(handle);
        }

        /// <summary>
        /// エディタ実行時用のModelViewerSetupDataの更新処理
        /// </summary>
        private void UpdateModelViewerSetupData(ModelViewerSetupData setupData) {
#if UNITY_EDITOR
            var ids = new HashSet<string>();
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(PreviewActorSetupData)}");
            foreach (var guid in guids) {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var assetKey = fileName.Replace("dat_preview_actor_setup_", "");
                ids.Add(assetKey);
            }

            setupData.actorDataIds = ids.OrderBy(x => x).ToArray();
            UnityEditor.EditorUtility.SetDirty(setupData);
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}
