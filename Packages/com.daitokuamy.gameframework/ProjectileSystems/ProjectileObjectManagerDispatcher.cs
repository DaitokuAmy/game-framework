using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ProjectileObjectManagerに生成されるRootにつけるDispatcher
    /// </summary>
    public class ProjectileObjectManagerDispatcher : MonoBehaviour {
        [SerializeField, Tooltip("Poolを使わないフラグ")]
        private bool _unusedPool;
        
        // 参照先のVfxManager
        public ProjectileObjectManager Manager { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(ProjectileObjectManager manager) {
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