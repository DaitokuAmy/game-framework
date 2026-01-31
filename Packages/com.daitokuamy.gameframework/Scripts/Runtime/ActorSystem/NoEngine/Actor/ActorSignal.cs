namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター用のシグナル(イベント情報)基底
    /// </summary>
    public class ActorSignal : IActorBufferContent {
        /// <inheritdoc/>
        int IActorBufferContent.Order => Order;
        
        /// <summary>処理順(低いほど優先)</summary>
        protected virtual int Order => 0;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorSignal() {}
        
        /// <inheritdoc/>
        void IActorBufferContent.OnCreated() => OnCreated();

        /// <inheritdoc/>
        void IActorBufferContent.OnReleased() => OnReleased();
        
        /// <summary>
        /// 生成時処理
        /// </summary>
        protected virtual void OnCreated() {}
        
        /// <summary>
        /// プール返却時処理
        /// </summary>
        protected virtual void OnReleased() {}
    }
}