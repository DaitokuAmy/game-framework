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
            void IState<StateType>.OnEnter(StateType prevKey, bool back, IScope scope) {
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
            void IState<StateType>.OnExit(StateType nextKey, bool back) {
                _coroutineRunner.StopAllCoroutines();
                ExitInternal(nextKey);
            }

            /// <summary>
            /// 移動入力
            /// </summary>
            public virtual void InputMove(Vector2 input) {
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
            protected void ChangeState(StateType nextKey, bool immediate = true) {
                Owner._stateContainer.Change(nextKey, immediate);
            }
        }

        /// <summary>
        /// ステートをまとめているクラス
        /// </summary>
        private class StateGroup {
            public LocomotionState Locomotion;

            public StateBase[] CreateStates(BattleCharacterActor owner) {
                return new StateBase[] {
                    Locomotion = new LocomotionState(owner),
                };
            }
        }

        private StateContainer<StateBase, StateType> _stateContainer;
        private StateGroup _stateGroup;

        /// <summary>現在のStateType</summary>
        public StateType CurrentStateType => _stateContainer.CurrentKey;
        
        /// <summary>現在のActionState</summary>
        private StateBase CurrentState => _stateContainer.FindState(_stateContainer.CurrentKey);

        /// <summary>
        /// ステートの初期化
        /// </summary>
        private void SetupStates(IScope scope) {
            _stateContainer?.Dispose();
            _stateContainer = new StateContainer<StateBase, StateType>().RegisterTo(scope);
            _stateGroup = new StateGroup();
            _stateContainer.Setup(
                StateType.Invalid,
                _stateGroup.CreateStates(this)
            );
            _stateContainer.Change(StateType.Locomotion, true);
        }

        /// <summary>
        /// ステートの更新
        /// </summary>
        private void UpdateState(float deltaTime) {
            _stateContainer.Update(deltaTime);
        }
    }
}