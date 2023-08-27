using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ProjectileObjectManagerに生成されるRootにつけるDispatcher
    /// </summary>
    public class ProjectileObjectManagerDispatcher : MonoBehaviour {
        // 参照先のVfxManager
        public ProjectileObjectManager Manager { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(ProjectileObjectManager manager) {
            Manager = manager;
        }
    }
}