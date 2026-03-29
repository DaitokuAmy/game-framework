using GameFramework;
using GameFramework.AssetSystem;
using GameFramework.UISystem;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// UIアセットの読み込み用ローダー
    /// </summary>
    public sealed class UIAssetLoader : IUIAssetLoader, System.IDisposable {
        private readonly SimpleAssetStorage _assetStorage;
        private readonly SimpleSceneStorage _sceneStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UIAssetLoader() {
            _assetStorage = new SimpleAssetStorage(AssetUtility.CreateAssetLoaders());
            _sceneStorage = new SimpleSceneStorage(AssetUtility.CreateSceneLoaders());
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            _sceneStorage.Dispose();
            _assetStorage.Dispose();
        }

        /// <inheritdoc/>
        ISceneProcess IUIAssetLoader.LoadSceneAsync(string key) {
            return _sceneStorage.LoadAsync(new UISceneRequest(key));
        }

        /// <inheritdoc/>
        IProcess<GameObject> IUIAssetLoader.LoadPrefabAsync(string key) {
            return _assetStorage.LoadAsync<GameObject, UIPrefabAssetRequest>(new UIPrefabAssetRequest(key));
        }

        /// <inheritdoc/>
        void IUIAssetLoader.UnloadScene(string key) {
            _sceneStorage.Unload(new UISceneRequest(key));
        }

        /// <inheritdoc/>
        void IUIAssetLoader.UnloadPrefab(string key) {
            _assetStorage.Unload<GameObject, UIPrefabAssetRequest>(new UIPrefabAssetRequest(key));
        }
    }
}
