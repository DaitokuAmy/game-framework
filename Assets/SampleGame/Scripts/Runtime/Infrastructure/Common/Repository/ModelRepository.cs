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
        public T CreateSingleModel<T>()
            where T : SingleModel<T>, new() {
            var type = typeof(T);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                storage = new SingleModelStorage<T>();
                _singleModelStorages[type] = storage;
            }

            return (T)storage.Create();
        }

        /// <summary>
        /// AutoIdModelの生成
        /// </summary>
        public T CreateAutoIdModel<T>(int startId = 1)
            where T : AutoIdModel<T>, new() {
            var type = typeof(T);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                storage = new AutoIdModelStorage<T>(startId);
                _autoIdModelStorages[type] = storage;
            }

            return (T)storage.Create();
        }

        /// <summary>
        /// IdModelの生成
        /// </summary>
        public T CreateIdModel<T>(int id)
            where T : IdModel<int, T>, new() {
            var type = typeof(T);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                storage = new IdModelStorage<int, T>();
                _idModelStorages[type] = storage;
            }

            return (T)storage.Create(id);
        }

        /// <summary>
        /// SingleModelの取得
        /// </summary>
        public T GetSingleModel<T>()
            where T : SingleModel<T>, new() {
            var type = typeof(T);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                return null;
            }

            return (T)storage.Get();
        }

        /// <summary>
        /// AutoIdModelの取得
        /// </summary>
        public T GetAutoIdModel<T>(int id)
            where T : AutoIdModel<T>, new() {
            var type = typeof(T);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return null;
            }

            return (T)storage.Get(id);
        }

        /// <summary>
        /// IdModelの取得
        /// </summary>
        public T GetIdModel<T>(int id)
            where T : IdModel<int, T>, new() {
            var type = typeof(T);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                return null;
            }

            return (T)storage.Get(id);
        }

        /// <summary>
        /// SingleModelの削除
        /// </summary>
        public void DeleteSingleModel<T>()
            where T : SingleModel<T>, new() {
            var type = typeof(T);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete();
        }

        /// <summary>
        /// AutoIdModelの削除
        /// </summary>
        public void DeleteAutoIdModel<T>(int id)
            where T : AutoIdModel<T>, new() {
            var type = typeof(T);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete(id);
        }

        /// <summary>
        /// AutoIdModelの削除
        /// </summary>
        public void DeleteAutoIdModel<T>(T model)
            where T : AutoIdModel<T>, new() {
            var type = typeof(T);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete(model.Id);
        }

        /// <summary>
        /// IdModelの削除
        /// </summary>
        public void DeleteIdModel<T>(int id)
            where T : IdModel<int, T>, new() {
            var type = typeof(T);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Delete(id);
        }

        /// <summary>
        /// SingleModelの全削除
        /// </summary>
        public void ClearSingleModels<T>()
            where T : SingleModel<T>, new() {
            var type = typeof(T);
            if (!_singleModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Clear();
        }

        /// <summary>
        /// AutoIdModelの全削除
        /// </summary>
        public void ClearAutoIdModels<T>()
            where T : AutoIdModel<T>, new() {
            var type = typeof(T);
            if (!_autoIdModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Clear();
        }

        /// <summary>
        /// IdModelの全削除
        /// </summary>
        public void ClearIdModels<T>()
            where T : IdModel<int, T>, new() {
            var type = typeof(T);
            if (!_idModelStorages.TryGetValue(type, out var storage)) {
                return;
            }

            storage.Clear();
        }
    }
}