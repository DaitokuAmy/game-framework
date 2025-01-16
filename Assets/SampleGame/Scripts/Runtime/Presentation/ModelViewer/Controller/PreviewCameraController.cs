using GameFramework.CameraSystems;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// モデルビューア用カメラ制御クラス
    /// </summary>
    public class PreviewCameraController : CameraController<PreviewCameraComponent> {
        private readonly IPreviewCameraMaster _master;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewCameraController(IPreviewCameraMaster master) {
            _master = master;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            // カメラ初期位置
            Component.LookAtOffset = _master.StartLookAtOffset;
            Component.AngleX = _master.StartAngles.x;
            Component.AngleY = _master.StartAngles.y;
            Component.Distance = _master.StartDistance;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            // 注視点リセットボタン
            if (Keyboard.current[Key.F].wasPressedThisFrame) {
                var resetOffset = Vector3.zero;
                if (Keyboard.current[Key.LeftShift].isPressed) {
                    resetOffset.y = Component.LookAtOffset.y;
                }

                Component.LookAtOffset = resetOffset;
            }

            // マウス移動による移動
            if (Mouse.current.leftButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _master.AngleControlSpeed;
                Component.AngleY += delta.x;
                Component.AngleX -= delta.y;
            }

            if (Mouse.current.rightButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _master.DistanceControlSpeed;
                Component.Distance -= delta.x + delta.y;
            }

            if (Mouse.current.middleButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _master.LookAtOffsetControlSpeed;
                var lookAtOffset = Component.LookAtOffset;
                lookAtOffset.x -= delta.x;
                lookAtOffset.y -= delta.y;
                Component.LookAtOffset = lookAtOffset;
            }

            var scroll = Mouse.current.scroll.ReadValue() * _master.ScrollDistanceControlDistanceControlSpeed;
            Component.Distance -= scroll.y;
        }
    }
}