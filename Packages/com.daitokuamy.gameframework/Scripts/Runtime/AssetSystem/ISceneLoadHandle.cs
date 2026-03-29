using System;
using GameFramework;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// シーン読み込み結果のハンドル
    /// </summary>
    public interface ISceneLoadHandle : IProcess<Scene>, IDisposable {
        /// <summary>読み込み結果</summary>
        Scene Scene { get; }

        /// <summary>有効なハンドルか</summary>
        bool IsValid { get; }

        /// <summary>
        /// シーンのアクティブ化
        /// </summary>
        AsyncOperationHandle ActivateAsync();

        /// <summary>
        /// 解放処理
        /// </summary>
        void Release();
    }
}
