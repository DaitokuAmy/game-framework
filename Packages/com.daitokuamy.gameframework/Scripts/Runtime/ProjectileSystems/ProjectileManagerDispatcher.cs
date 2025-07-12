using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ProjectileManagerに生成されるRootにつけるDispatcher
    /// </summary>
    public class ProjectileManagerDispatcher : MonoBehaviour {
        [SerializeField, Tooltip("Poolを使わないフラグ")]
        private bool _unusedPool;

        /// <summary>参照先のVfxManager</summary>
        public ProjectileManager Manager { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(ProjectileManager manager) {
            Manager = manager;
            ApplyStatus();
        }

        /// <summary>
        /// 値変化時
        /// </summary>
        private void OnValidate() {
            ApplyStatus();
        }

        /// <summary>
        /// Managerの状態に反映
        /// </summary>
        private void ApplyStatus() {
            if (Manager == null) {
                return;
            }

            Manager.SetActivePool(!_unusedPool);
        }
    }
}