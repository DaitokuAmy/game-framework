using System;
using UnityEngine;
using UnityEngine.Animations;

namespace SampleGame {
    /// <summary>
    /// ステータスイベント監視用リスナー
    /// </summary>
    public interface IStatusEventListener {
        /// <summary>
        /// ステータスに入った時の通知
        /// </summary>
        void OnStatusEnter(string statusName);

        /// <summary>
        /// ステータス内のループ回数変化時の通知
        /// </summary>
        void OnStatusCycle(string statusName, int cycle);

        /// <summary>
        /// ステータスから出た時の通知
        /// </summary>
        void OnStatusExit(string statusName);
    }

    /// <summary>
    /// 状態切り替わりの通知用Behaviour
    /// </summary>
    public class StatusEventStateMachineBehaviour : StateMachineBehaviour {
        // 通知マスク
        [Flags]
        private enum EventMasks {
            Enter = 1 << 0,
            Exit = 1 << 1,
            Cycle = 1 << 2,
        }

        [SerializeField, Tooltip("通知用のステータス名")]
        private string _status = "Empty";
        [SerializeField, Tooltip("通知用マスク")]
        private EventMasks _masks;

        private int _cycle;

        /// <summary>
        /// ステータスに入った際の処理
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
            AnimatorControllerPlayable controller) {
            base.OnStateEnter(animator, stateInfo, layerIndex, controller);

            _cycle = 0;

            if ((_masks & EventMasks.Enter) == 0) {
                return;
            }

            var listener = animator.GetComponentInParent<IStatusEventListener>();
            if (listener == null) {
                return;
            }

            listener.OnStatusEnter(_status);
        }

        /// <summary>
        /// ステータス更新中の処理
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
            AnimatorControllerPlayable controller) {
            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);

            var cycle = (int)stateInfo.normalizedTime;
            if (cycle == _cycle) {
                return;
            }

            _cycle = cycle;

            if ((_masks & EventMasks.Cycle) == 0) {
                return;
            }

            var listener = animator.GetComponentInParent<IStatusEventListener>();
            if (listener == null) {
                return;
            }

            listener.OnStatusCycle(_status, _cycle);
        }

        /// <summary>
        /// ステータス終了時の処理
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if ((_masks & EventMasks.Exit) == 0) {
                return;
            }

            var listener = animator.GetComponentInParent<IStatusEventListener>();
            if (listener == null) {
                return;
            }

            listener.OnStatusExit(_status);
        }

        /// <summary>
        /// ステートマシン開始時の処理
        /// </summary>
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash,
            AnimatorControllerPlayable controller) {
            base.OnStateMachineEnter(animator, stateMachinePathHash, controller);

            if ((_masks & EventMasks.Enter) == 0) {
                return;
            }

            var listener = animator.GetComponentInParent<IStatusEventListener>();
            if (listener == null) {
                return;
            }

            listener.OnStatusEnter(_status);
        }

        /// <summary>
        /// ステートマシン終了時の処理
        /// </summary>
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash,
            AnimatorControllerPlayable controller) {
            base.OnStateMachineExit(animator, stateMachinePathHash, controller);

            if ((_masks & EventMasks.Exit) == 0) {
                return;
            }

            var listener = animator.GetComponentInParent<IStatusEventListener>();
            if (listener == null) {
                return;
            }

            listener.OnStatusExit(_status);
        }
    }
}