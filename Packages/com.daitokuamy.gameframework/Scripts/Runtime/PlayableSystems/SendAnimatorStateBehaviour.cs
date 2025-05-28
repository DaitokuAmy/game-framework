using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimatorのStateを通知するStateMachineBehaviour
    /// </summary>
    public class SendAnimatorStateBehaviour : StateMachineBehaviour {
        private readonly Dictionary<Animator, AnimatorStateDispatcher> _animatorStates = new();
        
        /// <summary>
        /// State入った時の処理
        /// </summary>
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex,
            AnimatorControllerPlayable playable) {
            var dispatcher = GetDispatcher(animator);
            if (dispatcher == null) {
                return;
            }
            
            dispatcher.SendAnimatorStateEnter(stateInfo, layerIndex, playable);
        }

        /// <summary>
        /// State抜けた時の処理
        /// </summary>
        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex,
            AnimatorControllerPlayable playable) {
            var dispatcher = GetDispatcher(animator);
            if (dispatcher == null) {
                return;
            }
            
            dispatcher.SendAnimatorStateExit(stateInfo, layerIndex, playable);
        }

        /// <summary>
        /// StateMachine入った時の処理
        /// </summary>
        public override void OnStateMachineEnter(
            Animator animator,
            int stateMachinePathHash,
            AnimatorControllerPlayable playable) {
            var dispatcher = GetDispatcher(animator);
            if (dispatcher == null) {
                return;
            }
            
            dispatcher.SendAnimatorStateMachineEnter(stateMachinePathHash, playable);
        }

        /// <summary>
        /// StateMachine抜けた時の処理
        /// </summary>
        public override void OnStateMachineExit(
            Animator animator,
            int stateMachinePathHash,
            AnimatorControllerPlayable playable) {
            var dispatcher = GetDispatcher(animator);
            if (dispatcher == null) {
                return;
            }
            
            dispatcher.SendAnimatorStateMachineExit(stateMachinePathHash, playable);
        }

        /// <summary>
        /// 通知用のDispatcherの取得
        /// </summary>
        private AnimatorStateDispatcher GetDispatcher(Animator animator) {
            if (!_animatorStates.TryGetValue(animator, out var dispatcher)) {
                dispatcher = animator.GetComponent<AnimatorStateDispatcher>();
                _animatorStates[animator] = dispatcher;
            }

            return dispatcher;
        }
    }
}