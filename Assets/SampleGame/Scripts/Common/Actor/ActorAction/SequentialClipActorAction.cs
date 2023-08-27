using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;

namespace SampleGame {
    /// <summary>
    /// 連続再生するアニメーションクリップ用のActorAction
    /// </summary>
    [Serializable]
    public class SequentialClipActorAction : IActorAction {
        /// <summary>
        /// 再生クリップ情報
        /// </summary>
        [Serializable]
        public class ClipInfo {
            [Tooltip("再生対象のAnimationClip")]
            public AnimationClip animationClip;
            [Tooltip("同時再生するシーケンスクリップ")]
            public SequenceClip[] sequenceClips;
            [Tooltip("入りブレンド時間")]
            public float inBlend;
            [Tooltip("抜けブレンド時間")]
            public float outBlend;
        }

        [Tooltip("連続して流すクリップ")]
        public ClipInfo[] clipInfos;

        float IActorAction.OutBlend => clipInfos.Length > 0 ? clipInfos[clipInfos.Length - 1].outBlend : 0.0f;
    }
}