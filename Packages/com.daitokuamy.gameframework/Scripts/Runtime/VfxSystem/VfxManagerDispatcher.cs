using UnityEngine;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// VfxManagerに生成されるRootにつけるDispatcher
    /// </summary>
    public sealed class VfxManagerDispatcher : MonoBehaviour {
        [SerializeField, Tooltip("Poolを使わないフラグ")]
        private bool _unusedPool;
        
        /// <summary>参照先のVfxManager</summary>
        public VfxManager Manager { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(VfxManager vfxManager) {
            Manager = vfxManager;
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
