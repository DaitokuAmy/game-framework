using System;
using ActionSequencer;
using UnityEngine;
using GameFramework.ActorSystems;

namespace ThirdPersonEngine {
    /// <summary>
    /// AnimatorController用のActorAction
    /// </summary>
    [Serializable]
    public class ControllerActorAction : IActorAction {
        [Tooltip("再生対象のAnimatorController")]
        public RuntimeAnimatorController controller;
        [Tooltip("再生完了を表す最後のステート名")]
        public string lastStateName = "Last";
        [Tooltip("再生するシーケンスクリップ")]
        public SequenceClip sequenceClip;
        [Tooltip("入りブレンド時間")]
        public float inBlend = 0.1f;
        [Tooltip("抜けブレンド時間")]
        public float outBlend = 0.1f;
    }
}