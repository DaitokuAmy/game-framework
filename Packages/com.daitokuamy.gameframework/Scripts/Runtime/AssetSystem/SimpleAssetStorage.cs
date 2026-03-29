using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// 単純な AssetStorage
    /// </summary>
    public sealed class SimpleAssetStorage : AssetStorage {
        /// <summary>
        /// キャッシュキー
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
        /// キャッシュ情報
        /// </summary>
        private abstract class CacheInfo {
            /// <summary>有効か</summary>
            public abstract bool IsValid { get; }

            /// <summary>
            /// 解放
            /// </summary>
            public abstract void Release();
        }

        /// <summary>
        /// アセット別キャッシュ情報
        /// </summary>
        private sealed class CacheInfo<TAsset> : CacheInfo
            where TAsset : Object {
            /// <summary>アセット読み込みハンドル</summary>
            public readonly IAssetLoadHandle<TAsset> Handle;
            /// <summary>アセットプロセス</summary>
            public readonly AssetProcess<TAsset> Process;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CacheInfo(IAssetLoadHandle<TAsset> handle) {
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
        /// Handle を IProcess として扱うラッパー
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

        private readonly Dictionary<CacheKey, CacheInfo> _cacheInfos = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SimpleAssetStorage(params IAssetLoader[] loaders) : base(loaders) {
        }

        /// <inheritdoc/>
        public override IProcess<TAsset> LoadAsync<TAsset, TRequest>(TRequest request) {
            var cacheKey = new CacheKey(typeof(TAsset), request.Address);
            if (_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                return ((CacheInfo<TAsset>)cacheInfo).Process;
            }

            var handle = LoadAssetAsyncInternal<TAsset, TRequest>(request);
            var createdCacheInfo = new CacheInfo<TAsset>(handle);
            if (createdCacheInfo.IsValid) {
                _cacheInfos[cacheKey] = createdCacheInfo;
            }

            return createdCacheInfo.Process;
        }

        /// <inheritdoc/>
        public override void Unload<TAsset, TRequest>(TRequest request) {
            var cacheKey = new CacheKey(typeof(TAsset), request.Address);
            if (!_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                return;
            }

            cacheInfo.Release();
            _cacheInfos.Remove(cacheKey);
        }

        /// <inheritdoc/>
        public override void Clear() {
            foreach (var cacheInfo in _cacheInfos.Values) {
                cacheInfo.Release();
            }

            _cacheInfos.Clear();
        }

        /// <summary>
        /// 読み込み済みのアセット取得を試みます。
        /// </summary>
        public bool TryGet<TAsset>(AssetRequest<TAsset> request, out TAsset asset)
            where TAsset : Object {
            return TryGet<TAsset, AssetRequest<TAsset>>(request, out asset);
        }

        /// <summary>
        /// 読み込み済みのアセット取得を試みます。
        /// </summary>
        public bool TryGet<TAsset, TRequest>(TRequest request, out TAsset asset)
            where TAsset : Object
            where TRequest : struct, IAssetRequest<TAsset> {
            var cacheKey = new CacheKey(typeof(TAsset), request.Address);
            if (_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                asset = ((CacheInfo<TAsset>)cacheInfo).Handle.Asset;
                return asset != null;
            }

            asset = null;
            return false;
        }

        /// <summary>
        /// 読み込み済みのアセットを取得します。
        /// </summary>
        public TAsset GetAsset<TAsset>(AssetRequest<TAsset> request)
            where TAsset : Object {
            return GetAsset<TAsset, AssetRequest<TAsset>>(request);
        }

        /// <summary>
        /// 読み込み済みのアセットを取得します。
        /// </summary>
        public TAsset GetAsset<TAsset, TRequest>(TRequest request)
            where TAsset : Object
            where TRequest : struct, IAssetRequest<TAsset> {
            return TryGet<TAsset, TRequest>(request, out var asset) ? asset : null;
        }
    }
}
