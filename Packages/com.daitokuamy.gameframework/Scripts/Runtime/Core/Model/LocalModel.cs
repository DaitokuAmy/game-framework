namespace GameFramework.Core {
    /// <summary>
    /// ストレージ管理されないローカル用モデル
    /// </summary>
    public abstract class LocalModel<TModel> : IModel
        where TModel : LocalModel<TModel> {
        private DisposableScope _createScope;
        private bool _created;
        private bool _deleted;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected LocalModel() {
            OnCreated();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            OnDeleted();
        }

        /// <summary>
        /// 生成時処理(Override用)
        /// </summary>
        protected virtual void OnCreatedInternal(IScope scope) {
        }

        /// <summary>
        /// 削除時処理(Override用)
        /// </summary>
        protected virtual void OnDeletedInternal() {
        }

        /// <summary>
        /// 生成通知
        /// </summary>
        private void OnCreated() {
            if (_created || _deleted) {
                return;
            }

            _created = true;
            _deleted = false;

            _createScope = new();

            OnCreatedInternal(_createScope);
        }

        /// <summary>
        /// 削除通知
        /// </summary>
        private void OnDeleted() {
            if (!_created || _deleted) {
                return;
            }

            _deleted = true;
            _created = false;

            _createScope.Dispose();
            _createScope = null;

            OnDeletedInternal();
        }
    }
}