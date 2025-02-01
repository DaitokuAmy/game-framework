using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ModelSystems {
    /// <summary>
    /// 自動割り当てId管理によるモデル
    /// </summary>
    public abstract class AutoIdModel<TModel> : IModel
        where TModel : AutoIdModel<TModel> {
        /// <summary>
        /// GenericTypeCache
        /// </summary>
        private static class TypeCache<T> {
            // コンストラクタ
            public static ConstructorInfo ConstructorInfo { get; }

            static TypeCache() {
                ConstructorInfo = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(int) }, null);
            }
        }

        /// <summary>
        /// モデル格納用ストレージ
        /// </summary>
        private class Storage {
            private int _nextId = 1;

            // 管理対象のモデル
            private List<TModel> _items = new List<TModel>();
            public IReadOnlyCollection<TModel> Items => _items
                .Where(x => x != null)
                .ToArray();

            /// <summary>
            /// リセット処理
            /// </summary>
            public void Reset() {
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
            public T Create<T>()
                where T : TModel {
                var constructor = TypeCache<T>.ConstructorInfo;
                if (constructor == null) {
                    Debug.LogError($"Not found constructor. {typeof(T).Name}");
                    return null;
                }

                var id = _nextId++;
                var model = (T)constructor.Invoke(new object[] { id });
                _items.Add(model);
                model.OnCreatedInternal(model);
                return model;
            }

            /// <summary>
            /// モデルの取得
            /// </summary>
            /// <param name="id">モデルの識別キー</param>
            public T Get<T>(int id)
                where T : TModel {
                var index = IdToIndex(id);
                if (index < 0 || index >= _items.Count) {
                    return null;
                }

                return _items[index] as T;
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

        // インスタンス管理用クラス
        private static Storage s_storage = new();
        
        // キャンセル用
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // 管理中Modelリスト
        public static IReadOnlyCollection<TModel> Items => s_storage.Items;

        /// <summary>識別ID</summary>
        public int Id { get; private set; }
        /// <summary>キャンセル用トークン</summary>
        public CancellationToken Token => _cancellationTokenSource.Token;

        /// <summary>スコープ通知用</summary>
        public event Action OnExpired;

        /// <summary>
        /// 取得処理(継承先Model用)
        /// </summary>
        /// <param name="id">識別キー</param>
        public static T Get<T>(int id)
            where T : TModel {
            return s_storage.Get<T>(id);
        }

        /// <summary>
        /// 取得処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static TModel Get(int id) {
            return Get<TModel>(id);
        }

        /// <summary>
        /// 生成処理(継承先Model用)
        /// </summary>
        public static T Create<T>()
            where T : TModel {
            return s_storage.Create<T>();
        }

        /// <summary>
        /// 生成処理
        /// </summary>
        public static TModel Create() {
            return Create<TModel>();
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        /// <param name="id">識別キー</param>
        public static void Delete(int id) {
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
        protected AutoIdModel(int id) {
            Id = id;
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        private void OnDeleted() {
            OnDeletedInternal();
            Id = default;
            OnExpired?.InvokeDescending();
            OnExpired = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}