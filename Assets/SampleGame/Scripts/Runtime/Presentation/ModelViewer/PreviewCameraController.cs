using GameFramework.CameraSystems;
using SampleGame.Domain.ModelViewer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// モデルビューア用カメラ制御クラス
    /// </summary>
    public class PreviewCameraController : CameraController<PreviewCameraComponent> {
        private readonly IPreviewCameraControllerContext _context;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewCameraController(IPreviewCameraControllerContext context) {
            _context = context;
        }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            // 注視点リセットボタン
            if (Keyboard.current[Key.R].isPressed) {
                Component.LookAtOffset = Vector3.zero;
            }
            
            // マウス移動による移動
            if (Mouse.current.leftButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _context.MouseLeftDeltaSpeed;
                Component.AngleY += delta.x;
                Component.AngleX -= delta.y;
            }

            if (Mouse.current.rightButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _context.MouseRightDeltaSpeed;
                Component.Distance -= delta.x + delta.y;
            }

            if (Mouse.current.middleButton.isPressed) {
                var delta = Mouse.current.delta.ReadValue() * _context.MouseMiddleDeltaSpeed;
                var lookAtOffset = Component.LookAtOffset;
                lookAtOffset.x -= delta.x;
                lookAtOffset.y -= delta.y;
                Component.LookAtOffset = lookAtOffset;
            }

            var scroll = Mouse.current.scroll.ReadValue() * _context.MouseScrollSpeed;
            Component.Distance -= scroll.y;
        }
    }
}
