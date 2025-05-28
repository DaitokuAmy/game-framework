using System;
using System.Reflection;
using System.Threading;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ModelSystems {
    /// <summary>
    /// 自動割り当てId管理によるモデル
    /// </summary>
    public abstract class SingleModel<TModel> : IModel
        where TModel : SingleModel<TModel> {
        /// <summary>
        /// GenericTypeCache
        /// </summary>
        private static class TypeCache<T> {
            // コンストラクタ
            public static ConstructorInfo ConstructorInfo { get; }

            static TypeCache() {
                ConstructorInfo = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(object) }, null);
            }
        }

        /// <summary>
        /// モデル格納用ストレージ
        /// </summary>
        private class Storage {
            // 管理対象のモデル
            private TModel _item;
            public TModel Item => _item;

            /// <summary>
            /// リセット処理
            /// </summary>
            public void Reset() {
                var model = _item;
                if (model == null) {
                    return;
                }

                _item = null;
                model.OnDeleted();
            }

            /// <summary>
            /// モデルの生成
            /// </summary>
            public TModel Create() {
                if (_item != null) {
                    Debug.LogError($"Already exists {typeof(TModel).Name}.");
                    return null;
                }

                var constructor = TypeCache<TModel>.ConstructorInfo;
                if (constructor == null) {
                    Debug.LogError($"Not found constructor. {typeof(TModel).Name}");
                    return null;
                }

                var model = (TModel)constructor.Invoke(new object[] { null });
                _item = model;
                model.OnCreatedInternal(model);
                return model;
            }

            /// <summary>
            /// モデルの取得
            /// </summary>
            public TModel Get() {
                return _item;
            }

            /// <summary>
            /// モデルの削除
            /// </summary>
            public void Delete() {
                var model = _item;
                if (model == null) {
                    return;
                }

                _item = null;
                model.OnDeleted();
            }
        }

        // インスタンス管理用クラス
        private static Storage s_storage = new Storage();
        
        // キャンセル用
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // 管理中Model
        public static TModel Item => s_storage.Item;

        /// <summary>キャンセル用トークン</summary>
        public CancellationToken Token => _cancellationTokenSource.Token;

        /// <summary>スコープ通知用</summary>
        public event Action ExpiredEvent;

        /// <summary>
        /// 取得 or 生成処理
        /// </summary>
        public static TModel GetOrCreate() {
            var model = Get();
            if (model == null) {
                model = Create();
            }

            return model;
        }

        /// <summary>
        /// 取得処理
        /// </summary>
        public static TModel Get() {
            return s_storage.Get();
        }

        /// <summary>
        /// 生成処理
        /// </summary>
        public static TModel Create() {
            return s_storage.Create();
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        public static void Delete() {
            s_storage.Delete();
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
            Delete();
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
        /// <param name="empty">デフォルトコンストラクタを無効にするための空引数</param>
        protected SingleModel(object empty) {
        }

        /// <summary>
        /// 削除時処理
        /// </summary>
        private void OnDeleted() {
            OnDeletedInternal();
            ExpiredEvent?.InvokeDescending();
            ExpiredEvent = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}