using ActionSequencer;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// SequenceController提供用のMonoBehaviour
    /// </summary>
    public class SequenceControllerProvider : MonoBehaviour, ISequenceControllerProvider {
        // 再生に使用しているController
        public SequenceController SequenceController { get; private set; }

        /// <summary>
        /// SequenceControllerの設定
        /// </summary>
        public void SetSequenceController(SequenceController controller) {
            SequenceController = controller;
        }
    }
}