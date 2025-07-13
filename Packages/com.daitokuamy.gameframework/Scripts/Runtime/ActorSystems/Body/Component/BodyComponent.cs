using System;
using UnityEngine.Profiling;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body制御クラス基底
    /// </summary>
    public abstract class BodyComponent : IBodyComponent {
        private CustomSampler _updateSampler;
        private CustomSampler _lateUpdateSampler;

        /// <summary>実行優先度</summary>
        public virtual int ExecutionOrder => 0;
        
        /// <summary>制御対象のBody</summary>
        public Body Body { get; private set; }
        /// <summary>有効か</summary>
        public bool IsValid => Body != null;

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IBodyComponent.Initialize(Body body) {
            _updateSampler = CustomSampler.Create($"BodyComponent.{GetType().Name}.Update()");
            _lateUpdateSampler = CustomSampler.Create($"BodyComponent.{GetType().Name}.LateUpdate()");

            Body = body;
            InitializeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            DisposeInternal();
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
        protected virtual void InitializeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void LateUpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void IBodyComponent.Update(float deltaTime) {
            _updateSampler.Begin();
            UpdateInternal(deltaTime);
            _updateSampler.End();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBodyComponent.LateUpdate(float deltaTime) {
            _lateUpdateSampler.Begin();
            LateUpdateInternal(deltaTime);
            _lateUpdateSampler.End();
        }
    }
}