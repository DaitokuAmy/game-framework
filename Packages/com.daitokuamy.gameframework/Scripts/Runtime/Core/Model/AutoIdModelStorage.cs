using System.Collections.Generic;

namespace GameFramework.Core {
    /// <summary>
    /// AutoIdModelのストレージ
    /// </summary>
    public sealed class AutoIdModelStorage<TModel>
        where TModel : AutoIdModel<TModel>, new() {
        private readonly List<TModel> _items = new();

        private int _nextId = 1;

        /// <summary>モデルリスト</summary>
        public IReadOnlyCollection<TModel> Items => _items;

        /// <summary>
        /// クリア処理(全Modelの削除)
        /// </summary>
        public void Clear() {
            for (var i = 0; i < _items.Count; i++) {
                var model = _items[i];
                if (model == null) {
                    continue;
                }

                _items[i] = null;
                model.OnDeleted();
            }

            _items.Clear();
            _nextId = 1;
        }

        /// <summary>
        /// モデルの生成
        /// </summary>
        public TModel Create() {
            var id = _nextId++;
            var model = new TModel();
            _items[id] = model;
            model.OnCreated(this, id);
            return model;
        }

        /// <summary>
        /// モデルの取得
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        public TModel Get(int id) {
            var index = IdToIndex(id);
            if (index < 0 || index >= _items.Count) {
                return null;
            }

            return _items[index];
        }

        /// <summary>
        /// 含まれているか
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        public bool Contains(int id) {
            var index = IdToIndex(id);
            if (index < 0 || index >= _items.Count) {
                return false;
            }

            return _items[index] != null;
        }

        /// <summary>
        /// モデルの削除
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        public void Delete(int id) {
            var index = IdToIndex(id);
            if (index < 0 || index >= _items.Count) {
                return;
            }

            var model = _items[index];
            if (model == null) {
                return;
            }

            _items[index] = null;
            model.OnDeleted();
        }

        /// <summary>
        /// IdをIndexに変換
        /// </summary>
        private int IdToIndex(int id) {
            return id - 1;
        }
    }
}