using GameFramework.AssetSystem;
using UnityEngine;

namespace GameFramework.UISystem {    
    /// <summary>
    /// UIAssetを読み込みするためのインターフェース
    /// </summary>
    public interface IUIAssetLoader {
        /// <summary>
        /// 読み込み用のキーからScene読み込み結果を取得する
        /// </summary>
        ISceneProcess LoadSceneAsync(string key);

        /// <summary>
        /// 読み込み用のキーからPrefab読み込み結果を取得する
        /// </summary>
        IProcess<GameObject> LoadPrefabAsync(string key);

        /// <summary>
        /// 読み込み用のキーからSceneをアンロードする
        /// </summary>
        void UnloadScene(string key);

        /// <summary>
        /// 読み込み用のキーからPrefabをアンロードする
        /// </summary>
        void UnloadPrefab(string key);
    }
}
