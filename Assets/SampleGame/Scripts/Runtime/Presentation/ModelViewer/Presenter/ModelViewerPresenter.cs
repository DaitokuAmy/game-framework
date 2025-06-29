using GameFramework.CameraSystems;
using GameFramework;
using GameFramework.Core;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using R3;
using UnityEngine.InputSystem;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// ModelViewer全体のPresenter
    /// </summary>
    public class ModelViewerPresenter : Logic {
        private ModelViewerAppService _appService;
        private ActorEntityManager _actorEntityManager;
        private EnvironmentManager _environmentManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerPresenter() {
            _appService = Services.Resolve<ModelViewerAppService>();
            _actorEntityManager = Services.Resolve<ActorEntityManager>();
            _environmentManager = Services.Resolve<EnvironmentManager>();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var ct = scope.Token;

            var cameraManager = Services.Resolve<CameraManager>();
            var actorEntityManager = Services.Resolve<ActorEntityManager>();
            var domainService = _appService.DomainService;
            var modelViewerModel = domainService.ModelViewerModel;
            var settingsModel = domainService.SettingsModel;
            
            // 環境の切り替え
            modelViewerModel.CreatedEnvironmentSubject
                .TakeUntil(scope)
                .Subscribe(dto => {
                    // モデルビューアのSlot位置を変更
                    actorEntityManager.RootTransform.position = dto.EnvironmentModel.Master.RootPosition;
                    actorEntityManager.RootTransform.eulerAngles = dto.EnvironmentModel.Master.RootAngles;
                    
                    // AngleRootを変更
                    var angleRoot = cameraManager.GetTargetPoint("AngleRoot");
                    angleRoot.position = dto.EnvironmentModel.Master.RootPosition;
                    angleRoot.eulerAngles = dto.EnvironmentModel.Master.RootAngles;
                });

            // カメラの切り替え
            settingsModel.ChangedCameraControlTypeSubject
                .TakeUntil(scope)
                .Prepend(settingsModel.CameraControlType)
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

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            // モーションの再適用
            if (Keyboard.current[Key.Space].wasPressedThisFrame) {
                _appService.ReplayAnimationClip();
            }

            // モーションのIndex更新
            if (Keyboard.current[Key.UpArrow].wasPressedThisFrame) {
                _appService.PreviousAnimationClip();
            }

            if (Keyboard.current[Key.DownArrow].wasPressedThisFrame) {
                _appService.NextAnimationClip();
            }
            
            // CameraのModelTransformを更新
            var actorModel = _appService.DomainService.ActorModel;
            if (actorModel != null) { 
                var modelTrans = Services.Resolve<CameraManager>().GetTargetPoint("Model");
                modelTrans.position = actorModel.Position;
                modelTrans.rotation = actorModel.Rotation;
            }
        }
    }
}