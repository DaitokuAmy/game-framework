using GameAiBehaviour;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// BehaviourTreeController提供用のMonoBehaviour
    /// </summary>
    public class BehaviourTreeControllerProvider : MonoBehaviour, IBehaviourTreeControllerProvider {
        // 再生に使用しているController
        public BehaviourTreeController BehaviourTreeController { get; private set; }

        /// <summary>
        /// BehaviourTreeControllerの設定
        /// </summary>
        public void SetBehaviourTreeController(BehaviourTreeController controller) {
            BehaviourTreeController = controller;
        }
    }
}