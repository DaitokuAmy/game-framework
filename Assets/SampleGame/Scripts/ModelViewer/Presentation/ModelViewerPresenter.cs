using Cysharp.Threading.Tasks;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.LogicSystems;
using UniRx;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// ModelViewer全体のPresenter
    /// </summary>
    public class ModelViewerPresenter : Logic {
        private ModelViewerModel _model;
        private ActorManager _actorManager;
        private EnvironmentManager _environmentManager;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerPresenter(ModelViewerModel model) {
            _model = model;
            _actorManager = Services.Get<ActorManager>();
            _environmentManager = Services.Get<EnvironmentManager>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var ct = scope.Token;

            var cameraManager = Services.Get<CameraManager>();
            var settingsModel = _model.SettingsModel;
            
            // Actorの切り替え
            _model.PreviewActorModel.SetupData
                .TakeUntil(scope)
                .Subscribe(_ => {
                    _actorManager.ChangePreviewActor(_model.PreviewActorModel);
                });
            
            // Environmentの切り替え
            _model.EnvironmentModel.AssetId
                .TakeUntil(scope)
                .Subscribe(id => {
                    _environmentManager.ChangeEnvironmentAsync(id, ct).Forget();
                });

            // カメラの切り替え
            settingsModel.CameraControlType
                .TakeUntil(scope)
                .Subscribe(type => {
                    switch (type) {
                        case CameraControlType.Default:
                            cameraManager.ForceDeactivate("SceneView");
                            cameraManager.ForceActivate("Default");
                            break;
                        case CameraControlType.SceneView:
                            cameraManager.ForceActivate("SceneView");
                            cameraManager.ForceDeactivate("Default");
                            break;
                    }
                });
        }
    }
}
