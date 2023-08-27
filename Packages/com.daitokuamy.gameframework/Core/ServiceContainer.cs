using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のコンテナ
    /// </summary>
    public class ServiceContainer : IServiceContainer {
        // 管理用サービス
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private Dictionary<Type, List<object>> _serviceLists = new Dictionary<Type, List<object>>();

        // 登録解除用の廃棄可能インスタンスリスト(登録順)
        private List<IDisposable> _disposableServices = new List<IDisposable>();

        // 自動Disposeフラグ
        private bool _autoDispose;

        // 親のContainer
        private IServiceContainer _parent;
        // 子のContainer
        private List<IServiceContainer> _children = new List<IServiceContainer>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent">親ServiceContainer</param>
        /// <param name="autoDispose">登録したServiceを自動Disposeするか</param>
        public ServiceContainer(IServiceContainer parent = null, bool autoDispose = true) {
            if (parent == null && GetType() != typeof(Services)) {
                parent = Services.Instance;
            }

            _autoDispose = autoDispose;
            _parent = parent;

            if (_parent is ServiceContainer container) {
                container._children.Add(this);
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            ClearInternal();
        }

        /// <summary>
        /// サービスの設定
        /// </summary>
        /// <param name="type">紐づけ用の型</param>
        /// <param name="service">登録するインスタンス</param>
        public void Set<TClass>(Type type, TClass service)
            where TClass : class {
            if (_services.ContainsKey(type)) {
                Debug.LogError($"Already set service. Type:{type}");
                return;
            }

            _services[type] = service;
            if (service is IDisposable disposable) {
                _disposableServices.Add(disposable);
            }
        }

        /// <summary>
        /// サービスの設定
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        public void Set<T, TClass>(TClass service)
            where TClass : class {
            Set(typeof(T), service);
        }

        /// <summary>
        /// サービスの設定
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        public void Set<TClass>(TClass service)
            where TClass : class {
            Set(service.GetType(), service);
        }

        /// <summary>
        /// サービスの設定(複数登録するバージョン）
        /// </summary>
        /// <param name="type">紐づけ用の型</param>
        /// <param name="service">登録するインスタンス</param>
        /// <param name="index">インデックス</param>
        public void Set<TClass>(Type type, TClass service, int index)
            where TClass : class {
            if (!_serviceLists.TryGetValue(type, out var list)) {
                list = new List<object>();
                _serviceLists[type] = list;
            }

            while (index >= list.Count) {
                list.Add(null);
            }

            if (list[index] != null) {
                Debug.LogError($"Already set service. Type:{type} Index:{index}");
                return;
            }

            list[index] = service;
            if (service is IDisposable disposable) {
                _disposableServices.Add(disposable);
            }
        }

        /// <summary>
        /// サービスの設定(複数登録するバージョン）
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        /// <param name="index">インデックス</param>
        public void Set<T, TClass>(TClass service, int index)
            where TClass : class {
            Set(typeof(T), service, index);
        }

        /// <summary>
        /// サービスの設定(複数登録するバージョン）
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        /// <param name="index">インデックス</param>
        public void Set<TClass>(TClass service, int index)
            where TClass : class {
            Set(service.GetType(), service, index);
        }

        /// <summary>
        /// コンテナ内のクリア
        /// </summary>
        void IServiceContainer.Clear() {
            ClearInternal();
        }

        /// <summary>
        /// サービスの取得
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// </summary>
        object IServiceContainer.Get(Type type) {
            for (var i = _children.Count - 1; i >= 0; i--) {
                var result = _children[i].Get(type);
                if (result != default) {
                    return result;
                }
            }

            if (_services.TryGetValue(type, out var service)) {
                return service;
            }

            foreach (var pair in _services) {
                if (type.IsAssignableFrom(pair.Key)) {
                    return pair.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// サービスの取得
        /// </summary>
        T IServiceContainer.Get<T>() {
            return (T)((IServiceContainer)this).Get(typeof(T));
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <param name="index">インデックス</param>
        object IServiceContainer.Get(Type type, int index) {
            for (var i = _children.Count - 1; i >= 0; i--) {
                var result = _children[i].Get(type, index);
                if (result != default) {
                    return result;
                }
            }

            _serviceLists.TryGetValue(type, out var list);

            if (list == null) {
                foreach (var pair in _serviceLists) {
                    if (type.IsAssignableFrom(pair.Key)) {
                        list = pair.Value;
                    }
                }
            }

            if (list != null) {
                if (index < list.Count) {
                    return list[index];
                }

                Debug.LogError($"Invalid service index. Type:{type} Index:{index}");
            }

            return null;
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="index">インデックス</param>
        T IServiceContainer.Get<T>(int index) {
            return (T)((IServiceContainer)this).Get(typeof(T), index);
        }

        /// <summary>
        /// サービスの削除
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// </summary>
        void IServiceContainer.Remove(Type type) {
            if (!_services.TryGetValue(type, out var service)) {
                return;
            }

            if (_autoDispose) {
                if (service is IDisposable disposable) {
                    disposable.Dispose();
                    _disposableServices.Remove(disposable);
                }
            }

            _services.Remove(type);
        }

        /// <summary>
        /// サービスの削除
        /// </summary>
        void IServiceContainer.Remove<T>() {
            ((IServiceContainer)this).Remove(typeof(T));
        }

        /// <summary>
        /// サービスの削除
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <param name="index">インデックス</param>
        /// </summary>
        void IServiceContainer.Remove(Type type, int index) {
            if (!_serviceLists.TryGetValue(type, out var list)) {
                return;
            }

            if (index < 0 || index >= list.Count) {
                return;
            }

            if (_autoDispose) {
                if (list[index] is IDisposable disposable) {
                    disposable.Dispose();
                    _disposableServices.Remove(disposable);
                }
            }

            list[index] = null;
        }

        /// <summary>
        /// サービスの削除
        /// <param name="index">インデックス</param>
        /// </summary>
        void IServiceContainer.Remove<T>(int index) {
            ((IServiceContainer)this).Remove(typeof(T), index);
        }

        /// <summary>
        /// コンテナ内のクリア
        /// </summary>
        private void ClearInternal() {
            // 子を解放
            for (var i = _children.Count - 1; i >= 0; i--) {
                _children[i].Dispose();
            }

            _children.Clear();

            if (_autoDispose) {
                // 逆順に解放
                for (var i = _disposableServices.Count - 1; i >= 0; i--) {
                    var disposable = _disposableServices[i];
                    if (disposable == null) {
                        continue;
                    }

                    disposable.Dispose();
                }
            }

            // サービス参照をクリア
            _disposableServices.Clear();
            _services.Clear();
            _serviceLists.Clear();
        }
    }
}