using System;

namespace GameFramework.Core {
    /// <summary>
    /// SingleModelを管理するストレージのインターフェース
    /// </summary>
    public interface ISingleModelStorage : IModelStorage {
        /// <summary>存在している要素</summary>
        IModel Item { get; }

        /// <summary>
        /// モデルの追加
        /// </summary>
        void Add(IModel model);

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
        where TModel : SingleModel<TModel> {

        /// <inheritdoc/>
        IModel ISingleModelStorage.Item => Item;
        
        /// <summary>モデル</summary>
        public TModel Item { get; private set; }

        /// <inheritdoc/>
        void ISingleModelStorage.Add(IModel model) {
            if (model is TModel mdl) {
                Add(mdl);
            }
            else {
                throw new Exception($"Model is not {typeof(TModel).Name}. type:{model.GetType()}");
            }
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
        /// モデルの追加
        /// </summary>
        public void Add(TModel model) {
            if (Item != null) {
                throw new Exception($"Already exists {typeof(TModel).Name}.");
            }

            Item = model;
            model.OnCreated(this);
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