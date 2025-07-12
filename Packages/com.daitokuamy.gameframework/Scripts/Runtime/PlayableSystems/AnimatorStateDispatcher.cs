using System;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Motionを再生させるためのクラス
    /// </summary>
    public class AnimatorStateDispatcher : MonoBehaviour {
        /// <summary>
        /// State関連のイベント通知用情報
        /// </summary>
        public struct StateEventInfo {
            public AnimatorControllerPlayable Playable;
            public AnimatorStateInfo StateInfo;
            public int LayerIndex;

            public StateEventInfo(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
                Playable = playable;
                StateInfo = stateInfo;
                LayerIndex = layerIndex;
            }
        }

        /// <summary>
        /// StateMachine関連のイベント通知用情報
        /// </summary>
        public struct StateMachineEventInfo {
            public AnimatorControllerPlayable Playable;
            public int StateMachinePathHash;
            
            public StateMachineEventInfo(int stateMachinePathHash, AnimatorControllerPlayable playable) {
                Playable = playable;
                StateMachinePathHash = stateMachinePathHash;
            }
        }

        public event Action<StateEventInfo> AnimatorStateEnterEvent;
        public event Action<StateEventInfo> AnimatorStateExitEvent;
        public event Action<StateMachineEventInfo> AnimatorStateMachineEnterEvent;
        public event Action<StateMachineEventInfo> AnimatorStateMachineExitEvent;
        
        /// <summary>
        /// StateEnter通知の送信
        /// </summary>
        internal void SendAnimatorStateEnter(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
            AnimatorStateEnterEvent?.Invoke(new StateEventInfo(stateInfo, layerIndex, playable));
        }
        
        /// <summary>
        /// StateExit通知の送信
        /// </summary>
        internal void SendAnimatorStateExit(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
            AnimatorStateExitEvent?.Invoke(new StateEventInfo(stateInfo, layerIndex, playable));
        }
        
        /// <summary>
        /// StateMachineEnter通知の送信
        /// </summary>
        internal void SendAnimatorStateMachineEnter(int stateMachinePathHash, AnimatorControllerPlayable playable) {
            AnimatorStateMachineEnterEvent?.Invoke(new StateMachineEventInfo(stateMachinePathHash, playable));
        }
        
        /// <summary>
        /// StateMachineExit通知の送信
        /// </summary>
        internal void SendAnimatorStateMachineExit(int stateMachinePathHash, AnimatorControllerPlayable playable) {
            AnimatorStateMachineExitEvent?.Invoke(new StateMachineEventInfo(stateMachinePathHash, playable));
        }
    }
}