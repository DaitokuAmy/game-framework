using System;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// アセットリクエスト情報インターフェース
    /// </summary>
    public interface IAssetInfo<T> : IDisposable
        where T : Object {
        /// <summary>
        /// 読み込み完了しているか
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// 読み込み済みアセット
        /// </summary>
        T Asset { get; }

        /// <summary>
        /// エラー
        /// </summary>
        Exception Exception { get; }
    }
}