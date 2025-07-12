#if USE_R3
using R3;
using UnityEngine;
using UnityEngine.Animations;
#endif

#if USE_UNI_RX
using System;
using UniRx;
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
#elif USE_UNI_RX
        /// <summary>
        /// StateEnter通知の監視
        /// </summary>
        public static IObservable<AnimatorStateDispatcher.StateEventInfo> OnAnimatorStateEnterAsObservable(this AnimatorStateDispatcher self) {
            return UniRx.Observable.FromEvent<AnimatorStateDispatcher.StateEventInfo>(h => self.OnAnimatorStateEnterEvent += h, h => self.OnAnimatorStateEnterEvent -= h);
        }

        /// <summary>
        /// StateExit通知の監視
        /// </summary>
        public static IObservable<AnimatorStateDispatcher.StateEventInfo> OnAnimatorStateExitAsObservable(this AnimatorStateDispatcher self) {
            return UniRx.Observable.FromEvent<AnimatorStateDispatcher.StateEventInfo>(h => self.OnAnimatorStateExitEvent += h, h => self.OnAnimatorStateExitEvent -= h);
        }

        /// <summary>
        /// StateMachineEnter通知の監視
        /// </summary>
        public static IObservable<AnimatorStateDispatcher.StateMachineEventInfo> OnAnimatorStateMachineEnterAsObservable(this AnimatorStateDispatcher self) {
            return UniRx.Observable.FromEvent<AnimatorStateDispatcher.StateMachineEventInfo>(h => self.OnAnimatorStateMachineEnterEvent += h, h => self.OnAnimatorStateMachineEnterEvent -= h);
        }

        /// <summary>
        /// StateMachineExit通知の監視
        /// </summary>
        public static IObservable<AnimatorStateDispatcher.StateMachineEventInfo> OnAnimatorStateMachineExitAsObservable(this AnimatorStateDispatcher self) {
            return UniRx.Observable.FromEvent<AnimatorStateDispatcher.StateMachineEventInfo>(h => self.OnAnimatorStateMachineExitEvent += h, h => self.OnAnimatorStateMachineExitEvent -= h);
        }
#endif
    }
}