using System;
using GameFramework.Core;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセット格納クラス
    /// </summary>
    public abstract class AssetStorage : IDisposable {
        // アセット管理クラス
        private readonly AssetManager _assetManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        public AssetStorage(AssetManager assetManager) {
            _assetManager = assetManager;
        }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="request">読み込みリクエスト</param>
        /// <param name="unloadScope">アンロードトリガにするScope(NULL指定の場合は、自身でHandleをDispose)</param>
        protected AssetHandle<TAsset> LoadAssetAsyncInternal<TAsset>(AssetRequest<TAsset> request, IScope unloadScope = null)
            where TAsset : Object {
            return request.LoadAsync(_assetManager, unloadScope);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public abstract void Dispose();
    }
}