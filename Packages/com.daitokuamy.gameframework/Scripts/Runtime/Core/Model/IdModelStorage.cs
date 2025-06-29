using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.Core {
    /// <summary>
    /// Id管理によるモデル
    /// </summary>
    public sealed class IdModelStorage<TKey, TModel>
        where TModel : IdModel<TKey, TModel>, new() {
        private readonly Dictionary<TKey, TModel> _items = new();

        /// <summary>モデルリスト</summary>
        public IReadOnlyCollection<TModel> Items => _items.Values;

        /// <summary>
        /// クリア処理(全Modelの削除)
        /// </summary>
        public void Clear() {
            var keys = _items.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++) {
                var model = _items[keys[i]];
                if (model == null) {
                    return;
                }

                _items.Remove(keys[i]);
                model.OnDeleted();
            }
        }

        /// <summary>
        /// モデルの生成
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        public TModel Create(TKey id) {
            if (_items.ContainsKey(id)) {
                throw new Exception($"Already exists {typeof(TModel).Name}. key:{id}");
            }

            var model = new TModel();
            _items[id] = model;
            model.OnCreated(this, id);
            return model;
        }

        /// <summary>
        /// モデルの取得
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        public TModel Get(TKey id) {
            return _items.GetValueOrDefault(id);
        }

        /// <summary>
        /// 含まれているか
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        public bool Contains(TKey id) {
            return _items.ContainsKey(id);
        }

        /// <summary>
        /// モデルの削除
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        public void Delete(TKey id) {
            if (!_items.Remove(id, out var model)) {
                return;
            }

            model.OnDeleted();
        }
    }
}