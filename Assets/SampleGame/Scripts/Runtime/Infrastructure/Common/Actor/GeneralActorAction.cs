using System;
using UnityEngine;
using GameFramework.ActorSystems;
using UnityEngine.Serialization;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// 汎用的なActorAction
    /// </summary>
    [Serializable]
    public class GeneralActorAction {
        /// <summary>
        /// アクションのタイプ
        /// </summary>
        public enum ActionType {
            Clip,
            SequentialClip,
            Controller,
            Timeline,
            Timer,
            ReactionLoop,
        }
        
        [Tooltip("アクションタイプ")]
        public ActionType actionType = ActionType.Clip;
        [FormerlySerializedAs("clipActorAction")]
        [Tooltip("ClipベースのActorAction")]
        public AnimationClipActorAction animationClipActorAction;
        [Tooltip("連続的なClipベースのActorAction")]
        public SequentialClipActorAction sequentialClipActorAction;
        [Tooltip("ClipベースのActorAction")]
        public ControllerActorAction controllerActorAction;
        [Tooltip("TimelineベースのActorAction")]
        public TimelineActorAction timelineActorAction;
        [Tooltip("TimerベースのActorAction")]
        public TimerActorAction timerActorAction;
        [Tooltip("ReactionLoopClipベースのActorAction")]
        public ReactionLoopClipActorAction reactionLoopClipActorAction;

        /// <summary>
        /// Actionの取得
        /// </summary>
        public IActorAction GetAction() {
            switch (actionType) {
                case ActionType.Clip:
                    return animationClipActorAction;
                case ActionType.SequentialClip:
                    return sequentialClipActorAction;
                case ActionType.Controller:
                    return controllerActorAction;
                case ActionType.Timeline:
                    return timelineActorAction;
                case ActionType.Timer:
                    return timerActorAction;
                case ActionType.ReactionLoop:
                    return reactionLoopClipActorAction;
            }

            return null;
        }
    }
}