namespace GameFramework.Pooling {
    /// <summary>
    /// プール返却時に再利用可能な状態へ戻すためのインターフェース
    /// ※ObjectPool返却時にこのインターフェースを実装したクラスは関数が自動コールされる
    /// </summary>
    public interface IPoolable {
        /// <summary>
        /// プールに返却される際に呼ばれ、内部状態をリセットします。
        /// </summary>
        void ResetForPool();
    }
}