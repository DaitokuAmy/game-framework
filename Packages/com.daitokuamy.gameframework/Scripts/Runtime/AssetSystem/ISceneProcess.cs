using GameFramework;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// シーン読み込み結果
    /// </summary>
    public interface ISceneProcess : IProcess<Scene> {
        /// <summary>読み込み結果</summary>
        Scene Scene { get; }
        /// <summary>有効な結果か</summary>
        bool IsValid { get; }

        /// <summary>
        /// シーンをアクティブ化
        /// </summary>
        AsyncOperationHandle ActivateAsync();
    }
}
