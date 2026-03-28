using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// AnimatorのStateを通知するStateMachineBehaviour
    /// </summary>
    public sealed class SendAnimatorStateBehaviour : StateMachineBehaviour {
        private readonly Dictionary<Animator, AnimatorStateDispatcher> _dispatchers = new();
        
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
            if (!_dispatchers.TryGetValue(animator, out var dispatcher)) {
                dispatcher = animator.GetComponent<AnimatorStateDispatcher>();
                _dispatchers[animator] = dispatcher;
            }

            return dispatcher;
        }
    }
}
