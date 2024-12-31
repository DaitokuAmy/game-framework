using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;
using UnityEngine.Timeline;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// アニメーションクリップ用のActorAction
    /// </summary>
    [Serializable]
    public class TimelineActorAction : IActorAction {
        [Tooltip("再生するTimelineAssetの名前")]
        public TimelineAsset timelineAsset;
        [Tooltip("シーケンスクリップ情報")]
        public SequenceClip[] sequenceClips;
        [Tooltip("入りブレンド時間")]
        public float inBlend = 0.1f;
        [Tooltip("抜けブレンド時間")]
        public float outBlend = 0.1f;
    }
}