using GameFramework.OldModelSystems;
using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyRecordingModel {
        /// <summary>録画オプションマスク</summary>
        public RecordingOptions Options { get; }
        /// <summary>回転時間</summary>
        public float RotationDuration { get; }
    }
    
    /// <summary>
    /// 録画用モデル
    /// </summary>
    public class RecordingModel : SingleModel<RecordingModel>, IReadOnlyRecordingModel {
        /// <summary>録画オプションマスク</summary>
        public RecordingOptions Options { get; private set; } = RecordingOptions.ActorRotation;
        /// <summary>回転時間</summary>
        public float RotationDuration { get; private set; } = 2.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private RecordingModel(object empty) 
            : base(empty) {}
        
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
    }
}
