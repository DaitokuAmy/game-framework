using System;
using GameFramework;
using UnityEngine;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// アセット読み込み結果のハンドル
    /// </summary>
    public interface IAssetLoadHandle<TAsset> : IProcess<TAsset>, IDisposable
        where TAsset : UnityEngine.Object {
        /// <summary>読み込み結果</summary>
        TAsset Asset { get; }

        /// <summary>有効なハンドルか</summary>
        bool IsValid { get; }

        /// <summary>
        /// 解放処理
        /// </summary>
        void Release();
    }
}
