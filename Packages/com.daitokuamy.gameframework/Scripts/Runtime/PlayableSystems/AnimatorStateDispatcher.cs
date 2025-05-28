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

        public event Action<StateEventInfo> OnAnimatorStateEnterEvent;
        public event Action<StateEventInfo> OnAnimatorStateExitEvent;
        public event Action<StateMachineEventInfo> OnAnimatorStateMachineEnterEvent;
        public event Action<StateMachineEventInfo> OnAnimatorStateMachineExitEvent;
        
        /// <summary>
        /// StateEnter通知の送信
        /// </summary>
        internal void SendAnimatorStateEnter(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
            OnAnimatorStateEnterEvent?.Invoke(new StateEventInfo(stateInfo, layerIndex, playable));
        }
        
        /// <summary>
        /// StateExit通知の送信
        /// </summary>
        internal void SendAnimatorStateExit(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
            OnAnimatorStateExitEvent?.Invoke(new StateEventInfo(stateInfo, layerIndex, playable));
        }
        
        /// <summary>
        /// StateMachineEnter通知の送信
        /// </summary>
        internal void SendAnimatorStateMachineEnter(int stateMachinePathHash, AnimatorControllerPlayable playable) {
            OnAnimatorStateMachineEnterEvent?.Invoke(new StateMachineEventInfo(stateMachinePathHash, playable));
        }
        
        /// <summary>
        /// StateMachineExit通知の送信
        /// </summary>
        internal void SendAnimatorStateMachineExit(int stateMachinePathHash, AnimatorControllerPlayable playable) {
            OnAnimatorStateMachineExitEvent?.Invoke(new StateMachineEventInfo(stateMachinePathHash, playable));
        }
    }
}