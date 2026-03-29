using System;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// AssetStorage の基底クラス
    /// </summary>
    public abstract class AssetStorage : IDisposable {
        private readonly IAssetLoader[] _loaders;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected AssetStorage(params IAssetLoader[] loaders) {
            _loaders = loaders ?? Array.Empty<IAssetLoader>();
        }

        /// <inheritdoc/>
        public virtual void Dispose() {
            Clear();
        }

        /// <summary>
        /// アセットを読み込みます。
        /// </summary>
        public IProcess<TAsset> LoadAsync<TAsset>(AssetRequest<TAsset> request)
            where TAsset : Object {
            return LoadAsync<TAsset, AssetRequest<TAsset>>(request);
        }

        /// <summary>
        /// アセットを読み込みます。
        /// </summary>
        public abstract IProcess<TAsset> LoadAsync<TAsset, TRequest>(TRequest request)
            where TAsset : Object
            where TRequest : struct, IAssetRequest<TAsset>;

        /// <summary>
        /// アセットをアンロードします。
        /// </summary>
        public void Unload<TAsset>(AssetRequest<TAsset> request)
            where TAsset : Object {
            Unload<TAsset, AssetRequest<TAsset>>(request);
        }

        /// <summary>
        /// アセットをアンロードします。
        /// </summary>
        public abstract void Unload<TAsset, TRequest>(TRequest request)
            where TAsset : Object
            where TRequest : struct, IAssetRequest<TAsset>;

        /// <summary>
        /// すべてのアセットをアンロードします。
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// 解決した Loader で読み込みを開始します
        /// </summary>
        protected IAssetLoadHandle<TAsset> LoadAssetAsyncInternal<TAsset, TRequest>(TRequest request)
            where TAsset : Object
            where TRequest : struct, IAssetRequest<TAsset> {
            return ResolveLoader<TAsset, TRequest>(request).LoadAsync(request);
        }

        /// <summary>
        /// 読み込み可能な Loader を解決します
        /// </summary>
        private IAssetLoader ResolveLoader<TAsset, TRequest>(TRequest request)
            where TAsset : Object
            where TRequest : struct, IAssetRequest<TAsset> {
            if (!request.IsValid) {
                throw new ArgumentException("Request is not valid.", nameof(request));
            }

            for (var i = 0; i < _loaders.Length; i++) {
                var loader = _loaders[i];
                if (loader != null && loader.CanLoad(request)) {
                    return loader;
                }
            }

            throw new InvalidOperationException($"No asset loader can load '{request.Address}'.");
        }
    }
}
