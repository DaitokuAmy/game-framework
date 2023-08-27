using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ModelSystems {
    /// <summary>
    /// Id管理によるモデル
    /// </summary>
    public abstract class IdModel<TKey, TModel> : IModel
        where TModel : IdModel<TKey, TModel> {
        /// <summary>
        /// GenericTypeCache
        /// </summary>
        private static class TypeCache<T> {
            // コンストラクタ
            public static ConstructorInfo ConstructorInfo { get; }

            static TypeCache() {
                ConstructorInfo = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(TKey) }, null);
            }
        }

        /// <summary>
        /// モデル格納用ストレージ
        /// </summary>
        private class Storage {
            // 管理対象のモデル
            private Dictionary<TKey, TModel> _items = new Dictionary<TKey, TModel>();
            public IReadOnlyCollection<TModel> Items => _items.Values;

            /// <summary>
            /// リセット処理
            /// </summary>
            public void Reset() {
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
            public T Create<T>(TKey id)
                where T : TModel {
                if (_items.ContainsKey(id)) {
                    Debug.LogError($"Already exists {typeof(T).Name}. key:{id}");
                    return null;
                }

                var constructor = TypeCache<T>.ConstructorInfo;
                if (constructor == null) {
                    Debug.LogError($"Not found constructor. {typeof(T).Name}");
                    return null;
                }

                var model = (T)constructor.Invoke(new object[] { id });
                _items[id] = model;
                model.OnCreatedInternal(model);
                return model;
            }

            /// <summary>
            /// モデルの取得
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public T Get<T>(TKey id)
                where T : TModel {
                if (!_items.TryGetValue(id, out var model)) {
                    return null;
                }

                return model as T;
            }

            /// <summary>
            /// モデルの削除
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public void Delete(TKey id) {
                if (!_items.TryGetValue(id, out var model)) {
                    return;
                }

                _items.Remove(id);
                model.OnDeleted();
            }
        }

        // インスタンス管理用クラス
        private static Storage s_storage = new Storage();
        
        // キャンセル用
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // 管理中Modelリスト
        public static IReadOnlyCollection<TModel> Items => s_storage.Items;

        // 識別ID
        public TKey Id { get; private set; }
        /// <summary>キャンセル用トークン</summary>
        public CancellationToken Token => _cancellationTokenSource.Token;

        /// <summary>スコープ通知用</summary>
        public event Action OnExpired;

        /// <summary>
        /// 取得 or 生成処理(継承先Model用)
        /// </summary>
        /// <param name="id">識別キー</param>
        public static T GetOrCreate<T>(TKey id)
            where T : TModel {
            var model = Get<T>(id);
            if (model == null) {
                model = Create<T>(id);
            }

            return model;
        }

        /// <summary>
        /// 取得 or 生成処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel GetOrCreate(TKey id) {
            var model = Get<TModel>(id);
            if (model == null) {
                model = Create<TModel>(id);
            }

            return model;
        }

        /// <summary>
        /// 取得処理(継承先Model用)
        /// </summary>
        /// <param name="id">識別キー</param>
        public static T Get<T>(TKey id)
            where T : TModel {
            return s_storage.Get<T>(id);
        }

        /// <summary>
        /// 取得処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel Get(TKey id) {
            return Get<TModel>(id);
        }

        /// <summary>
        /// 生成処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static T Create<T>(TKey id)
            where T : TModel {
            return s_storage.Create<T>(id);
        }

        /// <summary>
        /// 生成処理(継承先Model用)
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel Create(TKey id) {
            return Create<TModel>(id);
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static void Delete(TKey id) {
            s_storage.Delete(id);
        }

        /// <summary>
        /// リセット処理
        /// </summary>
        public static void Reset() {
            s_storage.Reset();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Delete(Id);
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
        /// コンストラクタ
        /// </summary>
        protected IdModel(TKey id) {
            Id = id;
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        private void OnDeleted() {
            OnDeletedInternal();
            Id = default;
            OnExpired?.Invoke();
            OnExpired = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}