namespace GameFramework.Core {
    /// <summary>
    /// ストレージ管理されないローカル用モデルのId機能付きVer
    /// </summary>
    public abstract class IdLocalModel<TKey, TModel> : LocalModel<TModel>
        where TModel : IdLocalModel<TKey, TModel> {
        /// <summary>識別子</summary>
        public TKey Id { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected IdLocalModel(TKey id) {
            Id = id;
        }
    }
}