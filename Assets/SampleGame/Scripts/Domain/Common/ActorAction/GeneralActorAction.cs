using System;
using UnityEngine;
using GameFramework.ActorSystems;

namespace SampleGame.Domain.Common {
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
        }
        
        [Tooltip("アクションタイプ")]
        public ActionType actionType = ActionType.Clip;
        [Tooltip("ClipベースのActorAction")]
        public ClipActorAction clipActorAction;
        [Tooltip("連続的なClipベースのActorAction")]
        public SequentialClipActorAction sequentialClipActorAction;
        [Tooltip("ClipベースのActorAction")]
        public ControllerActorAction controllerActorAction;
        [Tooltip("TimelineベースのActorAction")]
        public TimelineActorAction timelineActorAction;

        /// <summary>
        /// Actionの取得
        /// </summary>
        public IActorAction GetAction() {
            switch (actionType) {
                case ActionType.Clip:
                    return clipActorAction;
                case ActionType.SequentialClip:
                    return sequentialClipActorAction;
                case ActionType.Controller:
                    return controllerActorAction;
                case ActionType.Timeline:
                    return timelineActorAction;
            }

            return null;
        }
    }
}