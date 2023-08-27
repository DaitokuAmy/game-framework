using System;
using ActionSequencer;
using GameFramework.ActorSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// TriggerコントロールによるActorAction
    /// </summary>
    [Serializable]
    public class TriggerStateActorAction : IActorAction {
        [Tooltip("トリガーに使うプロパティ名")]
        public string triggerPropertyName = "OnAction";
        [Tooltip("分岐用Indexのプロパティ名(未指定可)")]
        public string indexPropertyName = "";
        [Tooltip("分岐用Index")]
        public int index = 0;
        [Tooltip("最後のステート名")]
        public string lastStateName = "Last";
        [Tooltip("シーケンスクリップ情報")]
        public SequenceClip[] sequenceClips;

        float IActorAction.OutBlend => 0.0f;
    }
}