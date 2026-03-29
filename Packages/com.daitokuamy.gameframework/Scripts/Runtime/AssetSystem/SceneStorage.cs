using System;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// SceneStorage の基底クラス
    /// </summary>
    public abstract class SceneStorage : IDisposable {
        private readonly ISceneLoader[] _loaders;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected SceneStorage(params ISceneLoader[] loaders) {
            _loaders = loaders ?? Array.Empty<ISceneLoader>();
        }

        /// <inheritdoc/>
        public virtual void Dispose() {
            Clear();
        }

        /// <summary>
        /// シーンを読み込みます。
        /// </summary>
        public ISceneProcess LoadAsync(SceneRequest request) {
            return LoadAsync<SceneRequest>(request);
        }

        /// <summary>
        /// シーンを読み込みます。
        /// </summary>
        public abstract ISceneProcess LoadAsync<TRequest>(TRequest request)
            where TRequest : struct, ISceneRequest;

        /// <summary>
        /// シーンをアンロードします。
        /// </summary>
        public void Unload(SceneRequest request) {
            Unload<SceneRequest>(request);
        }

        /// <summary>
        /// シーンをアンロードします。
        /// </summary>
        public abstract void Unload<TRequest>(TRequest request)
            where TRequest : struct, ISceneRequest;

        /// <summary>
        /// すべてのシーンをアンロードします。
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// 解決した Loader で読み込みを開始します
        /// </summary>
        protected ISceneLoadHandle LoadSceneAsyncInternal<TRequest>(TRequest request)
            where TRequest : struct, ISceneRequest {
            return ResolveLoader(request).LoadAsync(request);
        }

        /// <summary>
        /// 読み込み可能な Loader を解決します
        /// </summary>
        private ISceneLoader ResolveLoader<TRequest>(TRequest request)
            where TRequest : struct, ISceneRequest {
            if (!request.IsValid) {
                throw new ArgumentException("Request is not valid.", nameof(request));
            }

            for (var i = 0; i < _loaders.Length; i++) {
                var loader = _loaders[i];
                if (loader != null && loader.CanLoad(request)) {
                    return loader;
                }
            }

            throw new InvalidOperationException($"No scene loader can load '{request.Address}'.");
        }
    }
}
