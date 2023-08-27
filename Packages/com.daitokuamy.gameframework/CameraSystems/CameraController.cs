using System;
using GameFramework.Core;

namespace GameFramework.CameraSystems {
    /// <summary>
    /// カメラ制御用コントローラのインターフェース
    /// </summary>
    public interface ICameraController : IDisposable {
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="component">制御に使うComponent</param>
        void Initialize(ICameraComponent component);

        /// <summary>
        /// アクティブ化
        /// </summary>
        void Activate();

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);
    }

    /// <summary>
    /// カメラ制御用コントローラー
    /// </summary>
    public abstract class CameraController<TComponent> : ICameraController
        where TComponent : class, ICameraComponent {
        private DisposableScope _scope = new();
        private DisposableScope _activeScope = new();

        private bool _isActive;

        // 制御に使うCameraComponent
        protected TComponent Component { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICameraController.Initialize(ICameraComponent component) {
            Component = component as TComponent;

            InitializeInternal(_scope);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (_scope == null || _scope.Disposed) {
                return;
            }

            ((ICameraController)this).Deactivate();
            DisposeInternal();
            _scope.Dispose();
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        void ICameraController.Activate() {
            if (_isActive) {
                return;
            }

            _isActive = true;
            ActivateInternal(_activeScope);
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void ICameraController.Deactivate() {
            if (!_isActive) {
                return;
            }

            _isActive = false;
            _activeScope.Clear();
            DeactivateInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ICameraController.Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="scope">廃棄時に消えるScope</param>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected virtual void ActivateInternal(IScope scope) {
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }
    }
}