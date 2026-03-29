using UnityEngine;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// 単一バックエンドに対するアセットローダー
    /// </summary>
    public interface IAssetLoader {
        /// <summary>
        /// 読み込み可能か
        /// </summary>
        bool CanLoad<TAsset>(IAssetRequest<TAsset> request)
            where TAsset : Object;

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        IAssetLoadHandle<TAsset> LoadAsync<TAsset>(IAssetRequest<TAsset> request)
            where TAsset : Object;
    }
}
