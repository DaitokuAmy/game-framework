using System;
using ActionSequencer;
using GameFramework.ActorSystems;
using UnityEngine;

namespace SampleGame.Domain.Common {
    /// <summary>
    /// TriggerコントロールによるActorAction
    /// </summary>
    [Serializable]
    public class TriggerStateActorAction : IActorAction {
        [Tooltip("AnimatorControllerのLayerIndex")]
        public int layerIndex = 0;
        [Tooltip("トリガーに使うプロパティ名")]
        public string triggerPropertyName = "OnAction";
        [Tooltip("分岐用Indexのプロパティ名(未指定可)")]
        public string indexPropertyName = "";
        [Tooltip("分岐用Index")]
        public int index = 0;
        [Tooltip("最後のステート名")]
        public string lastStateName = "Last";
        [Tooltip("最後のステートタグ")]
        public string lastTag = "";
        [Tooltip("シーケンスクリップ情報")]
        public SequenceClip[] sequenceClips;
    }
}