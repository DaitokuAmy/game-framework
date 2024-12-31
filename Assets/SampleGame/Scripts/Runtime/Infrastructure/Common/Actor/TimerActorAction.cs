using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// 時間待機用のActorAction
    /// </summary>
    [Serializable]
    public class TimerActorAction : IActorAction {
        [Tooltip("秒数")]
        public float duration;
        [Tooltip("同時再生するシーケンスクリップ")]
        public SequenceClip[] sequenceClips;
        [Tooltip("キャンセルされた時に流すシーケンスクリップ")]
        public SequenceClip[] cancelSequenceClips;
    }
}