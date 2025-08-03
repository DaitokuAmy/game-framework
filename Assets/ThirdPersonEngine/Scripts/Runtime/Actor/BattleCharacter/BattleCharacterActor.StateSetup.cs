using System.Collections;
using GameFramework;
using GameFramework.Core;
using UnityEngine;
using Coroutine = GameFramework.Coroutine;

namespace ThirdPersonEngine {
    /// <summary>
    /// バトルキャラアクター - State初期化
    /// </summary>
    partial class BattleCharacterActor {
        /// <summary>
        /// 状態タイプ
        /// </summary>
        public enum StateType {
            /// <summary>無効値</summary>
            Invalid = -1,
            /// <summary>通常移動状態</summary>
            Locomotion,
            /// <summary>ジャンプアクション</summary>
            JumpAction,
            /// <summary>攻撃アクション</summary>
            AttackAction,
        }

        /// <summary>
        /// ステート基底
        /// </summary>
        private abstract class StateBase : IState<StateType> {
            private readonly CoroutineRunner _coroutineRunner = new();

            private Coroutine _enterRoutine;

            /// <inheritdoc/>>
            StateType IState<StateType>.Key => StateType;

            /// <summary>ステートタイプ</summary>
            protected abstract StateType StateType { get; }

            /// <summary>制御対象のオーナー</summary>
            protected BattleCharacterActor Owner { get; }
            /// <summary>変移時間</summary>
            protected float DeltaTime => Owner.Body.DeltaTime;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            protected StateBase(BattleCharacterActor owner) {
                Owner = owner;
            }

            /// <summary>
            /// 入った時の処理
            /// </summary>
            void IState<StateType>.OnEnter(StateType prevKey, IScope scope) {
                _enterRoutine = _coroutineRunner.StartCoroutine(EnterRoutineInternal(prevKey, scope),
                    cancellationToken: scope.Token);
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            void IState<StateType>.OnUpdate(float deltaTime) {
                _coroutineRunner.Update();

                if (_enterRoutine.IsDone) {
                    UpdateInternal(deltaTime);
                }
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            void IState<StateType>.OnExit(StateType nextKey) {
                _coroutineRunner.StopAllCoroutines();
                ExitInternal(nextKey);
            }

            /// <summary>
            /// 移動入力
            /// </summary>
            public virtual void InputMove(Vector2 input) {
            }

            /// <summary>
            /// スプリント入力
            /// </summary>
            public virtual void InputSprint(bool sprint) {
            }

            /// <summary>
            /// ジャンプ入力
            /// </summary>
            public virtual void InputJump() {
            }

            /// <summary>
            /// 入り処理(非同期)
            /// </summary>
            protected virtual IEnumerator EnterRoutineInternal(StateType prevKey, IScope scope) {
                yield break;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            /// <param name="deltaTime">変移時間</param>
            protected virtual void UpdateInternal(float deltaTime) {
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            protected virtual void ExitInternal(StateType nextKey) {
            }

            /// <summary>
            /// ステートの変更
            /// </summary>
            protected void ChangeState(StateType nextKey) {
                Owner._fsm.Change(nextKey);
            }
        }

        /// <summary>
        /// ステートをまとめているクラス
        /// </summary>
        private class StateGroup {
            public LocomotionState Locomotion;
            public JumpActionState JumpAction;

            public StateBase[] CreateStates(BattleCharacterActor owner) {
                return new StateBase[] {
                    Locomotion = new LocomotionState(owner),
                    JumpAction = new JumpActionState(owner),
                };
            }
        }

        private FiniteStateMachine<StateBase, StateType> _fsm;
        private StateGroup _stateGroup;

        /// <summary>現在のStateType</summary>
        public StateType CurrentStateType => _fsm.CurrentKey;
        /// <summary>現在のActionState</summary>
        private StateBase CurrentState => _fsm.FindState(_fsm.CurrentKey);

        /// <summary>
        /// ステートの初期化
        /// </summary>
        private void SetupStates(IScope scope) {
            _fsm?.Dispose();
            _fsm = new FiniteStateMachine<StateBase, StateType>().RegisterTo(scope);
            _stateGroup = new StateGroup();
            _fsm.Setup(
                StateType.Invalid,
                _stateGroup.CreateStates(this)
            );
            _fsm.Change(StateType.Locomotion);
        }

        /// <summary>
        /// ステートの更新
        /// </summary>
        private void UpdateState(float deltaTime) {
            _fsm.Update(deltaTime);
        }
    }
}