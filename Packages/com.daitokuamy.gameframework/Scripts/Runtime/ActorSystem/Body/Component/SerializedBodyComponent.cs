using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// Body制御クラス基底
    /// </summary>
    public abstract class SerializedBodyComponent : MonoBehaviour, IBodyComponent {
        private CustomSampler _updateSampler;
        private CustomSampler _lateUpdateSampler;
        private DisposableScope _scope;
        private bool _disposed;

        /// <summary>実行優先度</summary>
        public virtual int ExecutionOrder => 0;
        /// <summary>制御対象のBody</summary>
        public Body Body { get; private set; }

        /// <inheritdoc/>
        void IBodyComponent.Initialize(Body body) {
            _updateSampler = CustomSampler.Create($"BodyController.{GetType().Name}.Update()");
            _lateUpdateSampler = CustomSampler.Create($"BodyController.{GetType().Name}.LateUpdate()");
            _scope = new DisposableScope();

            Body = body;
            InitializeInternal(_scope);
        }

        /// <inheritdoc/>
        void IDisposable.Dispose() {
            if (_disposed) {
                return;
            }
            
            _disposed = true;
            DisposeInternal();
            _scope.Dispose();
            Body = null;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <inheritdoc/>
        void IBodyComponent.Update(float deltaTime) {
            _updateSampler.Begin();
            UpdateInternal(deltaTime);
            _updateSampler.End();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <inheritdoc/>
        void IBodyComponent.LateUpdate(float deltaTime) {
            _lateUpdateSampler.Begin();
            LateUpdateInternal(deltaTime);
            _lateUpdateSampler.End();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void LateUpdateInternal(float deltaTime) {
        }
    }
}
