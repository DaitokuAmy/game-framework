using UnityEngine;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// VfxManagerに生成されるRootにつけるDispatcher
    /// </summary>
    public class VfxManagerDispatcher : MonoBehaviour {
        // 参照先のVfxManager
        public VfxManager Manager { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(VfxManager vfxManager) {
            Manager = vfxManager;
        }
    }
}