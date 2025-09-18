using GameFramework.Core;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyRecordingModel {
    }
    
    /// <summary>
    /// 録画用モデル
    /// </summary>
    public class RecordingModel : SingleModel<RecordingModel>, IReadOnlyRecordingModel {
        // /// <summary>録画オプションマスク</summary>
        // public ModelRecorder.Options Options { get; private set; } = ModelRecorder.Options.ActorRotation;
        /// <summary>回転時間</summary>
        public float RotationDuration { get; private set; } = 2.0f;
        
        // /// <summary>
        // /// 録画オプションの変更
        // /// </summary>
        // public void SetOptions(ModelRecorder.Options recordingOptions) {
        //     Options = recordingOptions;
        // }

        /// <summary>
        /// 回転時間の設定
        /// </summary>
        public void SetRotationDuration(float duration) {
            RotationDuration = FloatMath.Max(0.1f, duration);
        }
    }
}
