namespace GameFramework.ActorSystems {
    /// <summary>
    /// Actorを拡張するためのComponent基底
    /// </summary>
    public abstract class ActorComponent {
        /// <summary>実行順番(小さいほど先に実行)</summary>
        public virtual int ExecutionOrder => 0;

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate(float deltaTime) {
            LateUpdateInternal(deltaTime);
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
    }
}