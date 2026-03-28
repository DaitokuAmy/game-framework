using UnityEngine;

namespace GameFramework.CutsceneSystem {
    /// <summary>
    /// CutsceneManagerに生成されるRootにつけるDispatcher
    /// </summary>
    public class CutsceneManagerDispatcher : MonoBehaviour {
        /// <summary>参照先のCutsceneManager</summary>
        public CutsceneManager Manager { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(CutsceneManager cutsceneManager) {
            Manager = cutsceneManager;
        }
    }
}
