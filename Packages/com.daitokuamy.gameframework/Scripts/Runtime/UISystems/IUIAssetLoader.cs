using GameFramework.AssetSystems;
using UnityEngine;

namespace GameFramework.UISystems {    
    /// <summary>
    /// UIAssetを読み込みするためのインターフェース
    /// </summary>
    public interface IUIAssetLoader {
        /// <summary>
        /// 読み込み用のキーからScene読み込み用のAssetHandleを取得する
        /// </summary>
        SceneAssetHandle GetSceneAssetHandle(string key);

        /// <summary>
        /// 読み込み用のキーからPrefab読み込み用のAssetHandleを取得する
        /// </summary>
        AssetHandle<GameObject> GetPrefabAssetHandle(string key);
    }
}