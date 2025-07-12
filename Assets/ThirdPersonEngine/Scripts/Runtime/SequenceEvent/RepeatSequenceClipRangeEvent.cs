using ActionSequencer;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// SequenceClipを複数回実行するためのイベント
    /// </summary>
    public class RepeatSequenceClipRangeEvent : RangeSequenceEvent {
        [Tooltip("繰り返すSequenceClip")]
        public SequenceClip[] sequenceClips;
        [Tooltip("繰り返し回数")]
        public int count;
    }

    /// <summary>
    /// イベントハンドラ
    /// </summary>
    public class RepeatSequenceClipRangeEventHandler : RangeSequenceEventHandler<RepeatSequenceClipRangeEvent> {
        private SequenceController _sequenceController;
        
        private int _counter;
        private float _interval;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(SequenceController sequenceController) {
            _sequenceController = sequenceController;
        }

        /// <summary>
        /// 開始時処理
        /// </summary>
        protected override void OnEnter(RepeatSequenceClipRangeEvent sequenceEvent) {
            _interval = sequenceEvent.count <= 1 ? sequenceEvent.Duration : sequenceEvent.Duration / (sequenceEvent.count - 1);
            _counter = 0;
        }

        /// <summary>
        /// 更新時処理
        /// </summary>
        protected override void OnUpdate(RepeatSequenceClipRangeEvent sequenceEvent, float elapsedTime) {
            while (_counter < sequenceEvent.count && _counter <= (int)(elapsedTime / _interval)) {
                // 定期的にクリップを再生
                foreach (var clip in sequenceEvent.sequenceClips) {
                    _sequenceController.Play(clip);
                }

                _counter++;
            }
        }

        /// <summary>
        /// 終了時処理
        /// </summary>
        protected override void OnExit(RepeatSequenceClipRangeEvent sequenceEvent) {
            // 回数が目的数を満たしていない場合はここで実行
            while (_counter < sequenceEvent.count) {
                // 定期的にクリップを再生
                foreach (var clip in sequenceEvent.sequenceClips) {
                    _sequenceController.Play(clip);
                }

                _counter++;
            }
        }

        /// <summary>
        /// キャンセル時処理
        /// </summary>
        protected override void OnCancel(RepeatSequenceClipRangeEvent sequenceEvent) {
        }
    }
}