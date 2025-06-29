using System;

namespace GameFramework.Core {
    /// <summary>
    /// AutoIdModelのストレージ
    /// </summary>
    public sealed class SingleModelStorage<TModel>
        where TModel : SingleModel<TModel>, new() {
        /// <summary>モデル</summary>
        public TModel Item { get; private set; }

        /// <summary>
        /// クリア処理(全Modelの削除)
        /// </summary>
        public void Clear() {
            if (Item != null) {
                var item = Item;
                Item = null;
                item.OnDeleted();
            }
        }

        /// <summary>
        /// モデルの生成
        /// </summary>
        public TModel Create() {
            if (Item != null) {
                throw new Exception($"Already exists {typeof(TModel).Name}.");
            }
            
            Item = new TModel();
            Item.OnCreated(this);
            return Item;
        }

        /// <summary>
        /// モデルの取得
        /// </summary>
        public TModel Get() {
            return Item;
        }

        /// <summary>
        /// 存在しているか
        /// </summary>
        public bool IsExists() {
            return Item != null;
        }

        /// <summary>
        /// モデルの削除
        /// </summary>
        public void Delete() {
            if (Item == null) {
                return;
            }
            
            var item = Item;
            Item = null;
            item.OnDeleted();
        }
    }
}