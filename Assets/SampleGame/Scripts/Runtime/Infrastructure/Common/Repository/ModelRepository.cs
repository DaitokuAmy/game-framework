using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// モデル管理用リポジトリ
    /// </summary>
    public class ModelRepository : IModelRepository, IDisposable {
        private readonly Dictionary<Type, ISingleModelStorage> _singleModelStorages = new();
        private readonly Dictionary<Type, IAutoIdModelStorage> _autoIdModelStorages = new();
        private readonly Dictionary<Type, IIdModelStorage<int>> _idModelStorages = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelRepository() {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            foreach (var pair in _singleModelStorages) {
                pair.Value.Clear();
            }

            foreach (var pair in _autoIdModelStorages) {
                pair.Value.Clear();
            }

            foreach (var pair in _idModelStorages) {
                pair.Value.Clear();
            }

            _singleModelStorages.Clear();
            _autoIdModelStorages.Clear();
            _idModelStorages.Clear();
        }

        /// <summary>
        /// SingleModelの生成
        /// </summary>
        public TModel CreateSingleModel<TBaseModel, TModel>()
            where TBaseModel : SingleModel<TBaseModel>
            where TModel : TBaseModel, new() {
            var type = typeof(TBaseModel);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                storage = new SingleModelStorage<TBaseModel>();
                _singleModelStorages[type] = storage;
            }

            var model = new TModel();
            storage.Add(model);
            return model;
        }

        /// <summary>
        /// SingleModelの生成
        /// </summary>
        public TModel CreateSingleModel<TModel>()
            where TModel : SingleModel<TModel>, new() {
            return CreateSingleModel<TModel, TModel>();
        }

        /// <summary>
        /// AutoIdModelの生成
        /// </summary>
        public TModel CreateAutoIdModel<TBaseModel, TModel>(int startId = 1)
            where TBaseModel : AutoIdModel<TBaseModel>
            where TModel : TBaseModel, new() {
            var type = typeof(TBaseModel);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                storage = new AutoIdModelStorage<TBaseModel>(startId);
                _autoIdModelStorages[type] = storage;
            }

            var model = new TModel();
            storage.Add(model);
            return model;
        }

        /// <summary>
        /// AutoIdModelの生成
        /// </summary>
        public TModel CreateAutoIdModel<TModel>(int startId = 1)
            where TModel : AutoIdModel<TModel>, new() {
            return CreateAutoIdModel<TModel, TModel>(startId);
        }

        /// <summary>
        /// IdModelの生成
        /// </summary>
        public TModel CreateIdModel<TBaseModel, TModel>(int id)
            where TBaseModel : IdModel<int, TBaseModel>
            where TModel : TBaseModel, new() {
            var type = typeof(TBaseModel);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                storage = new IdModelStorage<int, TBaseModel>();
                _idModelStorages[type] = storage;
            }

            var model = new TModel();
            storage.Add(id, model);
            return model;
        }

        /// <summary>
        /// IdModelの生成
        /// </summary>
        public TModel CreateIdModel<TModel>(int id)
            where TModel : IdModel<int, TModel>, new() {
            return CreateIdModel<TModel, TModel>(id);
        }

        /// <summary>
        /// SingleModelの取得
        /// </summary>
        public TModel GetSingleModel<TModel>()
            where TModel : SingleModel<TModel> {
            var type = typeof(TModel);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                return null;
            }

            return (TModel)storage.Get();
        }

        /// <summary>
        /// AutoIdModelの取得
        /// </summary>
        public TModel GetAutoIdModel<TBaseModel, TModel>(int id)
            where TBaseModel : AutoIdModel<TBaseModel>
            where TModel : TBaseModel {
            var type = typeof(TBaseModel);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return null;
            }

            return storage.Get(id) as TModel;
        }

        /// <summary>
        /// AutoIdModelの取得
        /// </summary>
        public TModel GetAutoIdModel<TModel>(int id)
            where TModel : AutoIdModel<TModel> {
            return GetAutoIdModel<TModel, TModel>(id);
        }

        /// <summary>
        /// IdModelの取得
        /// </summary>
        public TModel GetIdModel<TBaseModel, TModel>(int id)
            where TBaseModel : IdModel<int, TBaseModel>
            where TModel : TBaseModel {
            var type = typeof(TBaseModel);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                return null;
            }

            return storage.Get(id) as TModel;
        }

        /// <summary>
        /// IdModelの取得
        /// </summary>
        public TModel GetIdModel<TModel>(int id)
            where TModel : IdModel<int, TModel> {
            return GetIdModel<TModel, TModel>(id);
        }

        /// <summary>
        /// SingleModelの削除
        /// </summary>
        public void DeleteSingleModel<TModel>()
            where TModel : SingleModel<TModel> {
            var type = typeof(TModel);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete();
        }

        /// <summary>
        /// AutoIdModelの削除
        /// </summary>
        public void DeleteAutoIdModel<TModel>(int id)
            where TModel : AutoIdModel<TModel> {
            var type = typeof(TModel);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete(id);
        }

        /// <summary>
        /// AutoIdModelの削除
        /// </summary>
        public void DeleteAutoIdModel<TModel>(TModel model)
            where TModel : AutoIdModel<TModel> {
            var type = typeof(TModel);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete(model.Id);
        }

        /// <summary>
        /// IdModelの削除
        /// </summary>
        public void DeleteIdModel<TModel>(int id)
            where TModel : IdModel<int, TModel> {
            var type = typeof(TModel);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete(id);
        }

        /// <summary>
        /// SingleModelの全削除
        /// </summary>
        public void ClearSingleModels<TModel>()
            where TModel : SingleModel<TModel> {
            var type = typeof(TModel);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Clear();
        }

        /// <summary>
        /// AutoIdModelの全削除
        /// </summary>
        public void ClearAutoIdModels<TModel>()
            where TModel : AutoIdModel<TModel> {
            var type = typeof(TModel);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Clear();
        }

        /// <summary>
        /// IdModelの全削除
        /// </summary>
        public void ClearIdModels<TModel>()
            where TModel : IdModel<int, TModel> {
            var type = typeof(TModel);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Clear();
        }
    }
}