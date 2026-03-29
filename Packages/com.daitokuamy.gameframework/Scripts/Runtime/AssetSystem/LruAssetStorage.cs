using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// LRU 方式のアセットストレージ
    /// </summary>
    public sealed class LruAssetStorage : AssetStorage {
        /// <summary>
        /// キャッシュの識別キー
        /// </summary>
        private readonly struct CacheKey : IEquatable<CacheKey> {
            /// <summary>アセット型</summary>
            public readonly Type AssetType;
            /// <summary>アセットアドレス</summary>
            public readonly string Address;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CacheKey(Type assetType, string address) {
                AssetType = assetType;
                Address = address;
            }

            /// <inheritdoc/>
            public bool Equals(CacheKey other) {
                return AssetType == other.AssetType && Address == other.Address;
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) {
                return obj is CacheKey other && Equals(other);
            }

            /// <inheritdoc/>
            public override int GetHashCode() {
                unchecked {
                    return ((AssetType?.GetHashCode() ?? 0) * 397) ^ (Address?.GetHashCode() ?? 0);
                }
            }
        }

        /// <summary>
        /// キャッシュ情報の基底クラス
        /// </summary>
        private abstract class CacheInfo {
            /// <summary>キャッシュキー</summary>
            public readonly CacheKey Key;
            /// <summary>キャッシュノード</summary>
            public LinkedListNode<CacheInfo> Node;
            /// <summary>キャッシュの有効状態</summary>
            public abstract bool IsValid { get; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            protected CacheInfo(CacheKey key) {
                Key = key;
            }

            /// <summary>
            /// 保持しているハンドルを解放
            /// </summary>
            public abstract void Release();
        }

        /// <summary>
        /// アセットごとのキャッシュ情報
        /// </summary>
        private sealed class CacheInfo<TAsset> : CacheInfo
            where TAsset : Object {
            /// <summary>アセットの読み込みハンドル</summary>
            public readonly IAssetLoadHandle<TAsset> Handle;
            /// <summary>アセットの読み込みプロセス</summary>
            public readonly AssetProcess<TAsset> Process;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CacheInfo(CacheKey key, IAssetLoadHandle<TAsset> handle)
                : base(key) {
                Handle = handle;
                Process = new AssetProcess<TAsset>(handle);
            }

            /// <inheritdoc/>
            public override bool IsValid => Handle.IsValid;

            /// <inheritdoc/>
            public override void Release() {
                Handle.Release();
            }
        }

        /// <summary>
        /// ハンドルを IProcess として扱うラッパー
        /// </summary>
        private sealed class AssetProcess<TAsset> : IProcess<TAsset>
            where TAsset : Object {
            private readonly IAssetLoadHandle<TAsset> _handle;

            /// <inheritdoc/>
            public bool IsDone => _handle.IsDone;
            /// <inheritdoc/>
            public Exception Exception => _handle.Exception;
            /// <inheritdoc/>
            public TAsset Result => _handle.Asset;
            /// <inheritdoc/>
            public object Current => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public AssetProcess(IAssetLoadHandle<TAsset> handle) {
                _handle = handle;
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
        private readonly Dictionary<CacheKey, CacheInfo> _cacheInfos = new();
        private readonly LinkedList<CacheInfo> _cacheOrder = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LruAssetStorage(int capacity, params IAssetLoader[] loaders)
            : base(loaders) {
            if (capacity <= 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");
            }

            _capacity = capacity;
        }

        /// <inheritdoc/>
        public override IProcess<TAsset> LoadAsync<TAsset, TRequest>(TRequest request) {
            var cacheKey = new CacheKey(typeof(TAsset), request.Address);
            if (_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                TouchCacheInfo(cacheInfo);
                return ((CacheInfo<TAsset>)cacheInfo).Process;
            }

            var handle = LoadAssetAsyncInternal<TAsset, TRequest>(request);
            var createdCacheInfo = new CacheInfo<TAsset>(cacheKey, handle);
            if (createdCacheInfo.IsValid) {
                AddCacheInfo(createdCacheInfo);
            }

            return createdCacheInfo.Process;
        }

        /// <inheritdoc/>
        public override void Unload<TAsset, TRequest>(TRequest request) {
            var cacheKey = new CacheKey(typeof(TAsset), request.Address);
            if (!_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
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
            _cacheInfos.Add(cacheInfo.Key, cacheInfo);
            TrimCache();
        }

        /// <summary>
        /// キャッシュを削除
        /// </summary>
        private void RemoveCacheInfo(CacheInfo cacheInfo) {
            cacheInfo.Release();
            _cacheInfos.Remove(cacheInfo.Key);

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
