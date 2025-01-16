using GameFramework.AssetSystems;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// UIアセットの読み込み用ローダー
    /// </summary>
    public class UIAssetLoader : IUIAssetLoader {
        private readonly AssetManager _assetManager;
    
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UIAssetLoader(AssetManager assetManager) {
            _assetManager = assetManager;
        }
    
        /// <summary>
        /// AssetHandleの取得
        /// </summary>
        SceneAssetHandle IUIAssetLoader.GetSceneAssetHandle(string key) {
            return new UISceneAssetRequest(key)
                .LoadAsync(_assetManager);
        }
    
        /// <summary>
        /// AssetHandleの取得
        /// </summary>
        AssetHandle<GameObject> IUIAssetLoader.GetPrefabAssetHandle(string key) {
            return new UIPrefabAssetRequest(key)
                .LoadAsync(_assetManager);
        }
    }
}