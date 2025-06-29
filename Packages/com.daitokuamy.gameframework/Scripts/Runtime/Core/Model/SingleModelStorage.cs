using System;

namespace GameFramework.Core {
    /// <summary>
    /// SingleModelを管理するストレージのインターフェース
    /// </summary>
    public interface ISingleModelStorage : IModelStorage {
        /// <summary>存在している要素</summary>
        IModel Item { get; }

        /// <summary>
        /// モデルの生成
        /// </summary>
        IModel Create();

        /// <summary>
        /// モデルの取得
        /// </summary>
        IModel Get();

        /// <summary>
        /// 存在しているか
        /// </summary>
        bool IsExists();

        /// <summary>
        /// モデルの削除
        /// </summary>
        void Delete();
    }
    
    /// <summary>
    /// AutoIdModelのストレージ
    /// </summary>
    public sealed class SingleModelStorage<TModel> : ISingleModelStorage
        where TModel : SingleModel<TModel>, new() {

        /// <inheritdoc/>
        IModel ISingleModelStorage.Item => Item;
        
        /// <summary>モデル</summary>
        public TModel Item { get; private set; }

        /// <inheritdoc/>
        IModel ISingleModelStorage.Create() {
            return Create();
        }

        /// <inheritdoc/>
        IModel ISingleModelStorage.Get() {
            return Get();
        }

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