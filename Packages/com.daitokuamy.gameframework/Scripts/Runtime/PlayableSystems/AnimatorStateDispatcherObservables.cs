#if USE_R3
using R3;
using UnityEngine;
using UnityEngine.Animations;
#endif

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimatorStateDispatcher用のReactive拡張
    /// </summary>
    public static class AnimatorStateDispatcherObservables {
#if USE_R3
        /// <summary>
        /// StateEnter通知の監視
        /// </summary>
        public static Observable<AnimatorStateDispatcher.StateEventInfo> OnAnimatorStateEnterAsObservable(this AnimatorStateDispatcher self) {
            return R3.Observable.FromEvent<AnimatorStateDispatcher.StateEventInfo>(h => self.AnimatorStateEnterEvent += h, h => self.AnimatorStateEnterEvent -= h);
        }

        /// <summary>
        /// StateExit通知の監視
        /// </summary>
        public static Observable<AnimatorStateDispatcher.StateEventInfo> OnAnimatorStateExitAsObservable(this AnimatorStateDispatcher self) {
            return R3.Observable.FromEvent<AnimatorStateDispatcher.StateEventInfo>(h => self.AnimatorStateExitEvent += h, h => self.AnimatorStateExitEvent -= h);
        }

        /// <summary>
        /// StateMachineEnter通知の監視
        /// </summary>
        public static Observable<AnimatorStateDispatcher.StateMachineEventInfo> OnAnimatorStateMachineEnterAsObservable(this AnimatorStateDispatcher self) {
            return R3.Observable.FromEvent<AnimatorStateDispatcher.StateMachineEventInfo>(h => self.AnimatorStateMachineEnterEvent += h, h => self.AnimatorStateMachineEnterEvent -= h);
        }

        /// <summary>
        /// StateMachineExit通知の監視
        /// </summary>
        public static Observable<AnimatorStateDispatcher.StateMachineEventInfo> OnAnimatorStateMachineExitAsObservable(this AnimatorStateDispatcher self) {
            return R3.Observable.FromEvent<AnimatorStateDispatcher.StateMachineEventInfo>(h => self.AnimatorStateMachineExitEvent += h, h => self.AnimatorStateMachineExitEvent -= h);
        }
#endif
    }
}