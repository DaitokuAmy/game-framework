using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// リアクションの成否を判定して制御するアニメーションクリップ用のActorAction
    /// </summary>
    [Serializable]
    public class ReactionLoopClipActorAction : IActorAction {
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

        [Tooltip("開始クリップ情報")]
        public ClipInfo inClipInfo;
        [Tooltip("ループクリップ情報")]
        public ClipInfo loopClipInfo;
        [Tooltip("アウトクリップ情報(成功)")]
        public ClipInfo successOutClipInfo;
        [Tooltip("アウトクリップ情報(失敗)")]
        public ClipInfo failureOutClipInfo;
        [Tooltip("キャンセルされた時に流すシーケンスクリップ")]
        public SequenceClip[] cancelSequenceClips;
    }
}