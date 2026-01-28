using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;

namespace ThirdPersonEngine {
    /// <summary>
    /// 時間待機用のActorAction
    /// </summary>
    [Serializable]
    public class TimerActorAction : IActorAction {
        [Tooltip("秒数")]
        public float duration;
        [Tooltip("同時再生するシーケンスクリップ")]
        public SequenceClip sequenceClip;
        [Tooltip("キャンセルされた時に流すシーケンスクリップ")]
        public SequenceClip cancelSequenceClip;
    }
}