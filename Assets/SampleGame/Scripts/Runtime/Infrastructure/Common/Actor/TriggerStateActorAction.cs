using System;
using ActionSequencer;
using GameFramework.ActorSystems;
using UnityEngine;

namespace SampleGame.Infrastructure {
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
        [Tooltip("最後のステートタグ名")]
        public string lastTag = "";
        [Tooltip("終了と見なす時間オフセット")]
        public float exitTimeOffset = 0.0f;
        [Tooltip("シーケンスクリップ情報")]
        public SequenceClip[] sequenceClips;
    }
}