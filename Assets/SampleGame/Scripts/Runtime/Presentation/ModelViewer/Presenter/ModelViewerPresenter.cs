using GameFramework.CameraSystems;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using R3;
using ThirdPersonEngine;
using ThirdPersonEngine.ModelViewer;
using UnityEngine.InputSystem;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// ModelViewer全体のPresenter
    /// </summary>
    public class ModelViewerPresenter : Logic {
        [ServiceInject]
        private ModelViewerAppService _appService;
        [ServiceInject]
        private ActorEntityManager _actorEntityManager;
        [ServiceInject]
        private CameraManager _cameraManager;
        [ServiceInject]
        private ModelRecorder _modelRecorder;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerPresenter() {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var ct = scope.Token;

            var domainService = _appService.DomainService;
            var modelViewerModel = domainService.ModelViewerModel;
            var settingsModel = domainService.SettingsModel;
            
            // 環境の切り替え
            modelViewerModel.ChangedEnvironmentActorSubject
                .TakeUntil(scope)
                .Subscribe(dto => {
                    // モデルビューアのSlot位置を変更
                    _actorEntityManager.RootTransform.position = dto.Model.RootPosition;
                    _actorEntityManager.RootTransform.rotation = dto.Model.RootRotation;
                    
                    // AngleRootを変更
                    var angleRoot = _cameraManager.GetTargetPoint("AngleRoot");
                    angleRoot.position = dto.Model.RootPosition;
                    angleRoot.rotation = dto.Model.RootRotation;
                    
                    // Recorderの同期
                    var environmentEntity = _actorEntityManager.FindEntity(dto.Model.Id);
                    var environmentActor = environmentEntity.GetActor<EnvironmentActor>();
                    _modelRecorder.LightSlot = environmentActor.LightSlot;
                });

            // カメラの切り替え
            settingsModel.ChangedCameraControlTypeSubject
                .TakeUntil(scope)
                .Prepend(settingsModel.CameraControlType)
                .Subscribe(type => {
                    switch (type) {
                        case CameraControlType.Default:
                            _cameraManager.ForceDeactivate("SceneView");
                            _cameraManager.ForceActivate("Default");
                            break;
                        case CameraControlType.SceneView:
                            _cameraManager.ForceActivate("SceneView");
                            _cameraManager.ForceDeactivate("Default");
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
            var actorModel = _appService.DomainService.PreviewActorModel;
            if (actorModel != null) { 
                var modelTrans = _cameraManager.GetTargetPoint("Model");
                modelTrans.position = actorModel.Position;
                modelTrans.rotation = actorModel.Rotation;
            }
        }
    }
}