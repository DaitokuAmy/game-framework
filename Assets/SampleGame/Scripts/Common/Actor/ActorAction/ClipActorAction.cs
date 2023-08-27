using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;

namespace SampleGame {
    /// <summary>
    /// アニメーションクリップ用のActorAction
    /// </summary>
    [Serializable]
    public class ClipActorAction : IActorAction {
        [Tooltip("再生対象のAnimationClip")]
        public AnimationClip animationClip;
        [Tooltip("同時再生するシーケンスクリップ")]
        public SequenceClip[] sequenceClips;
        [Tooltip("入りブレンド時間")]
        public float inBlend = 0.1f;
        [Tooltip("抜けブレンド時間")]
        public float outBlend = 0.1f;

        float IActorAction.OutBlend => outBlend;
    }
}