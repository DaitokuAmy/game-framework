using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// Bodyの生成と同時に付与されるMonoBehaviour
    /// </summary>
    public sealed class BodyDispatcher : MonoBehaviour {
        /// <summary>対象のBody</summary>
        public Body Body { get; private set; }

        /// <summary>
        /// 生成時処理
        /// </summary>
        public void Initialize(Body body) {
            Body = body;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            if (Body != null) {
                Body.Dispose();
                Body = null;
            }
        }
    }
}
