namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター用のコマンド(操作情報)基底
    /// </summary>
    public class ActorCommand : IActorBufferContent {
        /// <inheritdoc/>
        int IActorBufferContent.Order => Order;
        
        /// <summary>処理順(低いほど優先)</summary>
        protected virtual int Order => 0;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorCommand() {}
        
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