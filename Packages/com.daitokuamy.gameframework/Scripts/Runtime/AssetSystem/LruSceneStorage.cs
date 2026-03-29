using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// LRU 方式のシーンストレージ
    /// </summary>
    public sealed class LruSceneStorage : SceneStorage {
        /// <summary>
        /// キャッシュ情報
        /// </summary>
        private sealed class CacheInfo {
            /// <summary>シーンアドレス</summary>
            public readonly string Address;
            /// <summary>キャッシュノード</summary>
            public LinkedListNode<CacheInfo> Node;
            /// <summary>シーン読み込みハンドル</summary>
            public readonly ISceneLoadHandle Handle;
            /// <summary>シーン読み込み結果</summary>
            public readonly SceneProcess Process;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CacheInfo(string address, ISceneLoadHandle handle) {
                Address = address;
                Handle = handle;
                Process = new SceneProcess(handle);
            }

            /// <summary>有効か</summary>
            public bool IsValid => Handle.IsValid;

            /// <summary>
            /// 解放
            /// </summary>
            public void Release() {
                Handle.Release();
            }
        }

        /// <summary>
        /// Handle を ISceneProcess として扱うラッパー
        /// </summary>
        private sealed class SceneProcess : ISceneProcess {
            private readonly ISceneLoadHandle _handle;

            /// <inheritdoc/>
            public bool IsDone => _handle.IsDone;
            /// <inheritdoc/>
            public Exception Exception => _handle.Exception;
            /// <inheritdoc/>
            public Scene Scene => _handle.Scene;
            /// <inheritdoc/>
            public bool IsValid => _handle.IsValid;
            /// <inheritdoc/>
            public Scene Result => _handle.Scene;
            /// <inheritdoc/>
            public object Current => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public SceneProcess(ISceneLoadHandle handle) {
                _handle = handle;
            }

            /// <inheritdoc/>
            public AsyncOperationHandle ActivateAsync() {
                return _handle.ActivateAsync();
            }

            /// <inheritdoc/>
            public bool MoveNext() {
                return !_handle.IsDone;
            }

            /// <inheritdoc/>
            public void Reset() {
                throw new NotImplementedException();
            }
        }

        private readonly int _capacity;
        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();
        private readonly LinkedList<CacheInfo> _cacheOrder = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LruSceneStorage(int capacity, params ISceneLoader[] loaders)
            : base(loaders) {
            if (capacity <= 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");
            }

            _capacity = capacity;
        }

        /// <inheritdoc/>
        public override ISceneProcess LoadAsync<TRequest>(TRequest request) {
            if (_cacheInfos.TryGetValue(request.Address, out var cacheInfo)) {
                TouchCacheInfo(cacheInfo);
                if (request.ActivateOnLoad) {
                    cacheInfo.Process.ActivateAsync();
                }

                return cacheInfo.Process;
            }

            var handle = LoadSceneAsyncInternal(request);
            var createdCacheInfo = new CacheInfo(request.Address, handle);
            if (createdCacheInfo.IsValid) {
                AddCacheInfo(createdCacheInfo);
            }

            return createdCacheInfo.Process;
        }

        /// <inheritdoc/>
        public override void Unload<TRequest>(TRequest request) {
            if (!_cacheInfos.TryGetValue(request.Address, out var cacheInfo)) {
                return;
            }

            RemoveCacheInfo(cacheInfo);
        }

        /// <inheritdoc/>
        public override void Clear() {
            foreach (var cacheInfo in _cacheInfos.Values) {
                cacheInfo.Release();
            }

            _cacheInfos.Clear();
            _cacheOrder.Clear();
        }

        /// <summary>
        /// キャッシュを追加
        /// </summary>
        private void AddCacheInfo(CacheInfo cacheInfo) {
            cacheInfo.Node = _cacheOrder.AddLast(cacheInfo);
            _cacheInfos.Add(cacheInfo.Address, cacheInfo);
            TrimCache();
        }

        /// <summary>
        /// キャッシュを削除
        /// </summary>
        private void RemoveCacheInfo(CacheInfo cacheInfo) {
            cacheInfo.Release();
            _cacheInfos.Remove(cacheInfo.Address);

            if (cacheInfo.Node != null) {
                _cacheOrder.Remove(cacheInfo.Node);
                cacheInfo.Node = null;
            }
        }

        /// <summary>
        /// キャッシュの使用順を更新
        /// </summary>
        private void TouchCacheInfo(CacheInfo cacheInfo) {
            if (cacheInfo.Node == null || cacheInfo.Node.List != _cacheOrder || cacheInfo.Node == _cacheOrder.Last) {
                return;
            }

            _cacheOrder.Remove(cacheInfo.Node);
            cacheInfo.Node = _cacheOrder.AddLast(cacheInfo);
        }

        /// <summary>
        /// 上限を超えたキャッシュを解放
        /// </summary>
        private void TrimCache() {
            while (_cacheInfos.Count > _capacity) {
                var oldestNode = _cacheOrder.First;
                if (oldestNode == null) {
                    return;
                }

                RemoveCacheInfo(oldestNode.Value);
            }
        }
    }
}
