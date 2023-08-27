using GameFramework.ModelSystems;
using UnityEngine;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// 録画用モデル
    /// </summary>
    public class RecordingModel : AutoIdModel<RecordingModel> {
        /// <summary>録画オプションマスク</summary>
        public RecordingOptions Options { get; private set; } = RecordingOptions.ActorRotation;
        /// <summary>回転時間</summary>
        public float RotationDuration { get; private set; } = 2.0f;
        
        /// <summary>
        /// 録画オプションの変更
        /// </summary>
        public void SetOptions(RecordingOptions recordingOptions) {
            Options = recordingOptions;
        }

        /// <summary>
        /// 回転時間の設定
        /// </summary>
        public void SetRotationDuration(float duration) {
            RotationDuration = Mathf.Max(0.1f, duration);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private RecordingModel(int id) 
            : base(id) {}
    }
}
