using UnityEngine;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// アセット読み込みリクエスト
    /// </summary>
    public abstract class AssetRequest<TAsset>
        where TAsset : Object {
        /// <summary>読み込み用のAddress</summary>
        public abstract string Address { get; }
        /// <summary>読み込みに使用するProviderのキー配列（順番にフォールバック）</summary>
        public abstract string[] ProviderKeys { get; }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        /// <param name="unloadScope">解放スコープ</param>
        public AssetHandle<TAsset> LoadAsync(AssetManager assetManager, IScope unloadScope = null) {
            var address = Address;
            var handle = AssetHandle<TAsset>.Empty;

            // 読み込みに使用できるProviderを探し、それを使って読み込みを開始する
            for (var i = 0; i < ProviderKeys.Length; i++) {
                var provider = assetManager.GetProvider(ProviderKeys[i]);
                if (provider == null) {
                    continue;
                }

                var nextHandle = provider.LoadAsync<TAsset>(address);
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
                        return AssetHandle<TAsset>.Empty;
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
