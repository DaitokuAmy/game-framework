using System;
using ActionSequencer;
using GameFramework.ActorSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// CrossFadeコントロールによるActorAction
    /// </summary>
    [Serializable]
    public class CrossFadeStateActorAction : IActorAction {
        [Tooltip("AnimatorControllerのLayerIndex")]
        public int layerIndex = 0;
        [Tooltip("遷移先のステート名")]
        public string firstStateName = "First";
        [Tooltip("ブレンド時間")]
        public float inBlend = 0.1f;
        [Tooltip("最後のステート名")]
        public string lastStateName = "Last";
        [Tooltip("シーケンスクリップ情報")]
        public SequenceClip[] sequenceClips;


        float IActorAction.OutBlend => 0.0f;
    }
}