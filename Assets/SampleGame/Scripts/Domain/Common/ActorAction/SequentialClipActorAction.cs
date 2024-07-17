using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;

namespace SampleGame.Domain.Common {
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
            [Tooltip("Loopする場合の秒数(負の値で無限)")]
            public float loopDuration;
            [Tooltip("同時再生するシーケンスクリップ")]
            public SequenceClip[] sequenceClips;
            [Tooltip("入りブレンド時間")]
            public float inBlend;
            [Tooltip("抜けブレンド時間")]
            public float outBlend;
        }

        [Tooltip("連続して流すクリップ")]
        public ClipInfo[] clipInfos;
        [Tooltip("キャンセル時に実行させるシーケンスクリップ")]
        public SequenceClip[] cancelSequenceClips;
    }
}