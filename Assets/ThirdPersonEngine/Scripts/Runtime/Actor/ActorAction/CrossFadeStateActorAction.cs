using System;
using ActionSequencer;
using GameFramework.ActorSystems;
using UnityEngine;

namespace ThirdPersonEngine {
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
        [Tooltip("最後のステートタグ")]
        public string lastTag = "";
        [Tooltip("終了と見なす正規化再生時間")]
        public float exitNormalizedTime = 1.0f;
        [Tooltip("シーケンスクリップ情報")]
        public SequenceClip sequenceClip;
    }
}