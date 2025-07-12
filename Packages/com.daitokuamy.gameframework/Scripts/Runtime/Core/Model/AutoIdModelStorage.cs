using System.Collections.Generic;
using System.Linq;

namespace GameFramework.Core {
    /// <summary>
    /// AutoIdModelを管理するストレージのインターフェース
    /// </summary>
    public interface IAutoIdModelStorage : IModelStorage {
        /// <summary>存在している要素リスト</summary>
        IReadOnlyList<IModel> Items { get; }

        /// <summary>
        /// モデルの生成
        /// </summary>
        IModel Create();

        /// <summary>
        /// モデルの取得
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        IModel Get(int id);

        /// <summary>
        /// 含まれているか
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        bool Contains(int id);

        /// <summary>
        /// モデルの削除
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        void Delete(int id);
    }

    /// <summary>
    /// AutoIdModelのストレージ
    /// </summary>
    public sealed class AutoIdModelStorage<TModel> : IAutoIdModelStorage
        where TModel : AutoIdModel<TModel>, new() {
        private readonly List<TModel> _items = new();
        private readonly int _startId = 1;
        
        private int _nextId;

        /// <inheritdoc/>
        IReadOnlyList<IModel> IAutoIdModelStorage.Items => Items;

        /// <summary>モデルリスト</summary>
        public IReadOnlyList<TModel> Items => _items.Where(x => x != null).ToArray();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AutoIdModelStorage(int startId = 1) {
            _startId = startId;
            _nextId = _startId;
        }

        /// <inheritdoc/>
        IModel IAutoIdModelStorage.Create() {
            return Create();
        }

        /// <inheritdoc/>
        IModel IAutoIdModelStorage.Get(int id) {
            return Get(id);
        }

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
            _nextId = _startId;
        }

        /// <summary>
        /// モデルの生成
        /// </summary>
        public TModel Create() {
            var id = _nextId++;
            var model = new TModel();
            _items.Add(model);
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
            return id - _startId;
        }
    }
}