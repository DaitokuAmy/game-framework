using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// モデルビューア用の設定
    /// </summary>
    public class ModelViewerSettings : MonoBehaviour {
        [SerializeField, Tooltip("PreviewActorを配置する場所")]
        private Transform _previewSlot;
        
        /// <summary>PreviewActorを配置する場所</summary>
        public Transform PreviewSlot => _previewSlot;
    }
}
