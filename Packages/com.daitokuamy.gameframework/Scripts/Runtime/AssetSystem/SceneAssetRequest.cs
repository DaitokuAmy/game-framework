using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// シーンアセット読み込みリクエスト
    /// </summary>
    public abstract class SceneAssetRequest {
        /// <summary>読み込みモード</summary>
        public abstract LoadSceneMode Mode { get; }
        /// <summary>読み込み用のAddress</summary>
        public abstract string Address { get; }
        /// <summary>読み込みに使用するProviderのキー配列（順番にフォールバック）</summary>
        public abstract string[] ProviderKeys { get; }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        /// <param name="unloadScope">解放スコープ</param>
        public SceneAssetHandle LoadAsync(AssetManager assetManager, IScope unloadScope = null) {
            var address = Address;
            var handle = SceneAssetHandle.Empty;

            // 読み込みに使用できるProviderを探し、それを使って読み込みを開始する
            for (var i = 0; i < ProviderKeys.Length; i++) {
                var provider = assetManager.GetProvider(ProviderKeys[i]);
                if (provider == null) {
                    continue;
                }

                var nextHandle = provider.LoadSceneAsync(address, Mode);
                if (!nextHandle.IsValid) {
                    continue;
                }

                handle = nextHandle;
                break;
            }

            if (handle.IsValid) {
                if (unloadScope != null) {
                    if (!unloadScope.IsValid) {
                        handle.Release();
                        Debug.LogError($"Unload scope is already invalid. [{address}]");
                        return SceneAssetHandle.Empty;
                    }

                    // 解放処理を仕込む
                    unloadScope.ExpiredEvent += () => handle.Release();
                }
            }
            else {
                Debug.LogError($"Not found provider. [{address}]");
            }

            return handle;
        }
    }
}
