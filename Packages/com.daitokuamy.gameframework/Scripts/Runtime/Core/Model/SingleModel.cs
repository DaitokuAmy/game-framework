namespace GameFramework.Core {
    /// <summary>
    /// Singleton管理によるモデル
    /// </summary>
    public abstract class SingleModel<TModel> : IModel
        where TModel : SingleModel<TModel>, new() {
        private DisposableScope _createScope;
        private SingleModelStorage<TModel> _storage;
        private bool _created;
        private bool _deleted;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // Storage登録されていたら、そこ経由で削除
            if (_storage != null) {
                _storage.Delete();
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
        internal void OnCreated(SingleModelStorage<TModel> storage) {
            if (_created || _deleted) {
                return;
            }

            _created = true;
            _deleted = false;
            
            _createScope = new();
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
            
            _storage = null;
        }
    }
}