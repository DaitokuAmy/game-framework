using System.Collections;
using GameFramework.Core;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// バトルキャラアクター
    /// </summary>
    partial class BattleCharacterActor {
        /// <summary>
        /// JumpAction
        /// </summary>
        private sealed class JumpActionState : StateBase {
            /// <summary>ステートタイプ</summary>
            protected override StateType StateType => StateType.JumpAction;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public JumpActionState(BattleCharacterActor owner) : base(owner) {
            }

            /// <summary>
            /// 移動入力
            /// </summary>
            public override void InputMove(Vector2 input) {
                var direction = new Vector3(input.x, 0, input.y);
                direction = Owner.LookAtRotation * direction;
                var speedMultiplier = direction.sqrMagnitude * Owner._data.jumpActionInfo.moveSpeedScale;
                Owner.DirectionMove(direction, speedMultiplier);
            }

            /// <summary>
            /// 入り処理(非同期)
            /// </summary>
            protected override IEnumerator EnterRoutineInternal(StateType prevKey, IScope scope) {
                // 移動中か
                var isMoving = Owner.MoveComponent.IsMoving;
                
                // 重力をリセット
                Owner._velocityComponent.ResetGravity();

                // アクション再生
                var actionInfo = Owner._data.jumpActionInfo;
                var action = isMoving ? actionInfo.movingAction : actionInfo.standingAction;
                yield return Owner.PlayActionRoutine(action, scope.Token);
                
                // ロコモーションへ
                ChangeState(StateType.Locomotion);
            }
        }
    }
}