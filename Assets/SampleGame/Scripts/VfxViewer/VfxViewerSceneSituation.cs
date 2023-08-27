using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.CollisionSystems;
using GameFramework.Core;
using GameFramework.ProjectileSystems;
using GameFramework.SituationSystems;
using GameFramework.VfxSystems;
using UnityDebugSheet.Runtime.Core.Scripts;
using SampleGame.VfxViewer;

namespace SampleGame {
    /// <summary>
    /// モデルビューアシーン
    /// </summary>
    public class VfxViewerSceneSituation : SceneSituation {
        private int _debugPageId;
        
        protected override string SceneAssetPath => "vfx_viewer";

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            var ct = scope.Token;

            var settings = Services.Get<VfxViewerSettings>();
            
            // Modelの生成
            var model = VfxViewerModel.Create()
                .ScopeTo(scope);
            
            // Repositoryの生成
            var repository = new VfxViewerRepository(Services.Get<AssetManager>());
            repository.ScopeTo(scope);
            
            // ApplicationServiceの生成
            var appService = new VfxViewerApplicationService(model, repository);
            ServiceContainer.Set(appService);
            
            // ApplicationServiceの初期化
            yield return appService.SetupAsync(ct)
                .ToCoroutine();
            
            // 各種管理クラス生成/初期化
            var vfxManager = new VfxManager();
            vfxManager.RegisterTask(TaskOrder.Effect);
            ServiceContainer.Set(vfxManager);

            var collisionManager = new CollisionManager();
            collisionManager.RegisterTask(TaskOrder.Collision);
            ServiceContainer.Set(collisionManager);

            var projectileObjectManager = new ProjectileObjectManager(collisionManager);
            projectileObjectManager.RegisterTask(TaskOrder.Projectile);
            ServiceContainer.Set(projectileObjectManager);

            var environmentManager = new EnvironmentManager();
            ServiceContainer.Set(environmentManager);
            
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);

            var recordingController = Services.Get<RecordingController>();
            recordingController.RegisterTask(TaskOrder.Logic);
            
            // カメラ操作用Controllerの設定
            cameraManager.SetCameraController("Default", new PreviewCameraController());

            // Editor用のSetupData自動更新処理
            UpdateVfxViewerSetupData(model.SetupData);
                    
            // 初期状態反映
            appService.ChangeEnvironment(model.SetupData.defaultEnvironmentId);
            yield return appService.ChangePreviewVfxAsync(model.SetupData.defaultVfxDataId, ct)
                .ToCoroutine();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var ct = scope.Token;
            var model = VfxViewerModel.Get();
            var appService = Services.Get<VfxViewerApplicationService>();
            
            // Presenter初期化
            var presenter = new VfxViewerPresenter(model)
                .ScopeTo(scope);
            presenter.RegisterTask(TaskOrder.Logic);
            presenter.Activate();
            
            // DebugPage初期化
            var debugSheet = Services.Get<DebugSheet>();
            var rootPage = debugSheet.GetOrCreateInitialPage();
            _debugPageId = rootPage.AddPageLinkButton("Vfx Viewer", onLoad: pageTuple => {
                // Environment
                pageTuple.page.AddPageLinkButton("Environments", onLoad: fieldsPageTuple => {
                    var environmentIds = model.SetupData.environmentIds;
                    foreach (var environmentId in environmentIds) {
                        var id = environmentId;
                        fieldsPageTuple.page.AddButton(environmentId,
                            clicked: () => appService.ChangeEnvironment(id));
                    }
                });
                
                // PreviewVfx
                pageTuple.page.AddPageLinkButton("VfxList", onLoad: modelsPageTuple => {
                    var actorDataIds = model.SetupData.vfxDataIds;
                    foreach (var actorDataId in actorDataIds) {
                        var id = actorDataId;
                        modelsPageTuple.page.AddButton(actorDataId, clicked:() => {
                            appService.ChangePreviewVfxAsync(id, ct).Forget();
                        });
                    }
                });
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
        /// エディタ実行時用のVfxViewerSetupDataの更新処理
        /// </summary>
        private void UpdateVfxViewerSetupData(VfxViewerSetupData setupData) {
#if UNITY_EDITOR
            var ids = new HashSet<string>();
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(PreviewVfxSetupData)}");
            foreach (var guid in guids) {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var assetKey = fileName.Replace("dat_preview_vfx_setup_", "");
                ids.Add(assetKey);
            }

            setupData.vfxDataIds = ids.OrderBy(x => x).ToArray();
            UnityEditor.EditorUtility.SetDirty(setupData);
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}
