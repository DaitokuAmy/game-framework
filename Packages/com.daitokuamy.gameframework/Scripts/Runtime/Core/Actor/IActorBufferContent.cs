namespace GameFramework.Core {
    /// <summary>
    /// ActorBufferに詰め込むクラス用のインターフェース
    /// </summary>
    internal interface IActorBufferContent {
        /// <summary>処理順(低いほど優先)</summary>
        int Order { get; }
        
        /// <summary>
        /// 生成時の処理
        /// </summary>
        void OnCreated();
        
        /// <summary>
        /// Pool返却時の処理
        /// </summary>
        void OnReleased();
    }
}