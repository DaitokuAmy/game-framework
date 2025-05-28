using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセット提供用のクラス
    /// </summary>
    public interface IAssetProvider {
        /// <summary>
        /// アセットの読み込み
        /// </summary>
        AssetHandle<T> LoadAsync<T>(string address)
            where T : Object;

        /// <summary>
        /// アセットが含まれているか
        /// </summary>
        bool Contains<T>(string address);

        /// <summary>
        /// シーンアセットの読み込み
        /// </summary>
        SceneAssetHandle LoadSceneAsync(string address, LoadSceneMode mode);

        /// <summary>
        /// シーンアセットが含まれているか
        /// </summary>
        bool ContainsScene(string address);
    }
}