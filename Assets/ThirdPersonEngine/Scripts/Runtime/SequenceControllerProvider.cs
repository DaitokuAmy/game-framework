using ActionSequencer;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// SequenceController提供用のMonoBehaviour
    /// </summary>
    public class SequenceControllerProvider : MonoBehaviour, ISequenceControllerProvider {
        /// <summary>再生に使用しているController</summary>
        public SequenceController SequenceController { get; private set; }

        /// <summary>
        /// SequenceControllerの設定
        /// </summary>
        public static void SetController(GameObject gameObject, SequenceController sequenceController) {
            var provider = gameObject.GetComponent<SequenceControllerProvider>();
            if (provider == null) {
                provider = gameObject.AddComponent<SequenceControllerProvider>();
            }

            provider.SetControllerInternal(sequenceController);
        }

        /// <summary>
        /// SequenceControllerの設定
        /// </summary>
        private void SetControllerInternal(SequenceController controller) {
            SequenceController = controller;
        }
    }
}