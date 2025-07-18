using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.Core {
    /// <summary>
    /// IdModelを管理するストレージのインターフェース
    /// </summary>
    public interface IIdModelStorage<in TKey> : IModelStorage {
        /// <summary>存在している要素リスト</summary>
        IReadOnlyCollection<IModel> Items { get; }

        /// <summary>
        /// モデルの追加
        /// </summary>
        void Add(TKey id, IModel model);

        /// <summary>
        /// モデルの取得
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        IModel Get(TKey id);

        /// <summary>
        /// 含まれているか
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        bool Contains(TKey id);

        /// <summary>
        /// モデルの削除
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        void Delete(TKey id);
    }

    /// <summary>
    /// Id管理によるモデル
    /// </summary>
    public sealed class IdModelStorage<TKey, TModel> : IIdModelStorage<TKey>
        where TModel : IdModel<TKey, TModel> {
        private readonly Dictionary<TKey, TModel> _items = new();


        /// <inheritdoc/>
        IReadOnlyCollection<IModel> IIdModelStorage<TKey>.Items => Items;

        /// <summary>モデルリスト</summary>
        public IReadOnlyCollection<TModel> Items => _items.Values;

        /// <inheritdoc/>
        void IIdModelStorage<TKey>.Add(TKey id, IModel model) {
            if (model is TModel mdl) {
                Add(id, mdl);
            }
            else {
                throw new Exception($"Model is not {typeof(TModel).Name}. id:{id} type:{model.GetType()}");
            }
        }

        /// <inheritdoc/>
        IModel IIdModelStorage<TKey>.Get(TKey id) {
            return Get(id);
        }

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
        /// モデルの追加
        /// </summary>
        /// <param name="id">モデルの識別キー</param>
        /// <param name="model">追加するモデル</param>
        public void Add(TKey id, TModel model) {
            if (_items.ContainsKey(id)) {
                throw new Exception($"Already exists {typeof(TModel).Name}. key:{id}");
            }

            _items[id] = model;
            model.OnCreated(this, id);
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