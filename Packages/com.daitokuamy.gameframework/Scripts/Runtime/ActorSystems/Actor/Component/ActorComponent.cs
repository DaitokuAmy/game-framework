using System;
using GameFramework.Core;
using UnityEngine.Profiling;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Actorを拡張するためのComponent基底
    /// </summary>
    public abstract class ActorComponent : IDisposable {
        private bool _initialized;
        private bool _disposed;
        private DisposableScope _scope;
        private CustomSampler _updateSampler;
        private CustomSampler _lateUpdateSampler;

        /// <summary>実行順番(小さいほど先に実行)</summary>
        public virtual int ExecutionOrder => 0;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected virtual void LateUpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        protected virtual void DrawGizmosInternal() {
        }

        /// <summary>
        /// 選択中ギズモ描画
        /// </summary>
        protected virtual void DrawGizmosSelectedInternal() {
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        internal void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;
            _scope = new();

            _updateSampler = CustomSampler.Create($"ActorComponent.{GetType().Name}.Update()");
            _lateUpdateSampler = CustomSampler.Create($"ActorComponent.{GetType().Name}.LateUpdate()");

            InitializeInternal(_scope);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            DisposeInternal();

            if (_initialized) {
                _scope.Dispose();
                _scope = null;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        internal void Update(float deltaTime) {
            _updateSampler.Begin();
            UpdateInternal(deltaTime);
            _updateSampler.End();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        internal void LateUpdate(float deltaTime) {
            _lateUpdateSampler.Begin();
            LateUpdateInternal(deltaTime);
            _lateUpdateSampler.End();
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        internal void DrawGizmos() {
            DrawGizmosInternal();
        }

        /// <summary>
        /// 選択中ギズモ描画
        /// </summary>
        internal void DrawGizmosSelected() {
            DrawGizmosSelectedInternal();
        }
    }
}