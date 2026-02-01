using ActionSequencer;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// ログ出量用の範囲イベント
    /// </summary>
    public sealed class LogRangeSequenceEvent : RangeSequenceEvent {
        public string Text = "Hoge";
    }

    /// <summary>
    /// ハンドラ
    /// </summary>
    public sealed class LogRangeSequenceEventHandler : RangeSequenceEventHandler<LogRangeSequenceEvent> {
        /// <inheritdoc/>
        protected override void OnEnter(LogRangeSequenceEvent sequenceEvent) {
            Debug.Log($"Enter:{sequenceEvent.Text}");
        }

        /// <inheritdoc/>
        protected override void OnExit(LogRangeSequenceEvent sequenceEvent) {
            Debug.Log($"Exit:{sequenceEvent.Text}");
        }
    }
}