using GameFramework.Core;
using UnityEngine;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセット読み込みリクエスト
    /// </summary>
    public abstract class AssetRequest<TAsset>
        where TAsset : Object {
        // 読み込み用のAddress
        public abstract string Address { get; }
        // 読み込みに使用するProviderのIndex配列（順番にフォールバック）
        public abstract int[] ProviderIndices { get; }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        /// <param name="unloadScope">解放スコープ</param>
        public AssetHandle<TAsset> LoadAsync(AssetManager assetManager, IScope unloadScope = null) {
            var address = Address;
            var handle = AssetHandle<TAsset>.Empty;

            // 読み込みに使用できるProviderを探し、それを使って読み込みを開始する
            for (var i = 0; i < ProviderIndices.Length; i++) {
                var provider = assetManager.GetProvider(ProviderIndices[i]);
                if (provider == null) {
                    continue;
                }

                if (!provider.Contains<TAsset>(address)) {
                    continue;
                }

                handle = provider.LoadAsync<TAsset>(address);
                break;
            }

            if (handle.IsValid) {
                if (unloadScope != null) {
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