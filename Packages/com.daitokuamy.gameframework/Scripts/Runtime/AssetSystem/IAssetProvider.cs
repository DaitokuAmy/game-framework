using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// アセット提供用のクラス
    /// </summary>
    public interface IAssetProvider {
        /// <summary>
        /// Provider識別用キー
        /// </summary>
        string Key { get; }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        AssetHandle<T> LoadAsync<T>(string address)
            where T : Object;

        /// <summary>
        /// シーンアセットの読み込み
        /// </summary>
        SceneAssetHandle LoadSceneAsync(string address, LoadSceneMode mode);
    }
}
