using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シーンアセット情報インターフェース
    /// </summary>
    public interface ISceneAssetInfo : IDisposable {
        /// <summary>
        /// 読み込み完了しているか
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// シーン情報
        /// </summary>
        Scene Scene { get; }

        /// <summary>
        /// エラー
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// アクティブ化
        /// </summary>
        AsyncOperation ActivateAsync();
    }
}