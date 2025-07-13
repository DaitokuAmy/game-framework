namespace GameFramework.Core {
    /// <summary>
    /// 自動割り当てId管理によるモデル
    /// </summary>
    public abstract class AutoIdModel<TModel> : IModel
        where TModel : AutoIdModel<TModel>, new() {
        private DisposableScope _createScope;
        private AutoIdModelStorage<TModel> _storage;
        private bool _created;
        private bool _deleted;

        /// <summary>識別ID</summary>
        public int Id { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // Storage登録されていたら、そこ経由で削除
            if (_storage != null) {
                _storage.Delete(Id);
            }
            else {
                OnDeleted();
            }
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
        internal void OnCreated(AutoIdModelStorage<TModel> storage, int id) {
            if (_created || _deleted) {
                return;
            }

            _created = true;
            _deleted = false;

            _createScope = new();
            Id = id;
            _storage = storage;

            OnCreatedInternal(_createScope);
        }

        /// <summary>
        /// 削除通知
        /// </summary>
        internal void OnDeleted() {
            if (!_created || _deleted) {
                return;
            }

            _deleted = true;
            _created = false;

            _createScope.Dispose();
            _createScope = null;

            OnDeletedInternal();

            Id = 0;
            _storage = null;
        }
    }
}