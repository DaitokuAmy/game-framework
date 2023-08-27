using UnityEngine;

namespace GameFramework.CutsceneSystems {
    /// <summary>
    /// CutsceneManagerに生成されるRootにつけるDispatcher
    /// </summary>
    public class CutsceneManagerDispatcher : MonoBehaviour {
        // 参照先のCutsceneManager
        public CutsceneManager Manager { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(CutsceneManager cutsceneManager) {
            Manager = cutsceneManager;
        }
    }
}