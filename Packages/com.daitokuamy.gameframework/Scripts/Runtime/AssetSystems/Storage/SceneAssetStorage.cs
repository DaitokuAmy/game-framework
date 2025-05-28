using System;
using GameFramework.Core;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーンアセット格納クラス
    /// </summary>
    public abstract class SceneAssetStorage : IDisposable {
        // アセット管理クラス
        private readonly AssetManager _assetManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        public SceneAssetStorage(AssetManager assetManager) {
            _assetManager = assetManager;
        }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="request">読み込みリクエスト</param>
        /// <param name="unloadScope">アンロードトリガにするScope(NULL指定の場合は、自身でHandleをDispose)</param>
        protected SceneAssetHandle LoadAssetAsyncInternal(SceneAssetRequest request, IScope unloadScope = null) {
            return request.LoadAsync(_assetManager, unloadScope);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public abstract void Dispose();
    }
}