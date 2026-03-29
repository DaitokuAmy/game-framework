using UnityEngine;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// アセット読み込み要求
    /// </summary>
    public interface IAssetRequest<TAsset>
        where TAsset : Object {
        /// <summary>読み込み対象アドレス</summary>
        string Address { get; }

        /// <summary>有効な要求か</summary>
        bool IsValid { get; }
    }
}
