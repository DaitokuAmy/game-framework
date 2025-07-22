using System.Collections;
using GameFramework.Core;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// バトルキャラアクター
    /// </summary>
    partial class BattleCharacterActor {
        /// <summary>
        /// Locomotion
        /// </summary>
        private sealed class LocomotionState : StateBase {
            private Vector3 _moveDirection;
            private bool _isRun;
            private bool _isSprint;

            /// <summary>ステートタイプ</summary>
            protected override StateType StateType => StateType.Locomotion;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public LocomotionState(BattleCharacterActor owner) : base(owner) {
            }

            /// <summary>
            /// 移動入力
            /// </summary>
            public override void InputMove(Vector2 input) {
                // todo:視線を元に変換
                _moveDirection = new Vector3(input.x, 0, input.y);
                _isRun = _moveDirection.sqrMagnitude >= 0.5 * 0.5f;
                Owner.DirectionMove(_moveDirection, 0.0f);
            }

            /// <summary>
            /// スプリント入力
            /// </summary>
            public override void InputSprint(bool sprint) {
                _isSprint = sprint;
            }

            /// <summary>
            /// ジャンプ入力
            /// </summary>
            public override void InputJump() {
                ChangeState(StateType.JumpAction);
            }

            /// <summary>
            /// 入り処理(非同期)
            /// </summary>
            protected override IEnumerator EnterRoutineInternal(StateType prevKey, IScope scope) {
                _isSprint = false;

                Owner.MotionComponent.Change(Owner.BasePlayableComponent, 0.2f);
                yield break;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            /// <param name="deltaTime">変移時間</param>
            protected override void UpdateInternal(float deltaTime) {
                UpdateAnimationProperties();
            }

            /// <summary>
            /// アニメーション用プロパティの反映
            /// </summary>
            private void UpdateAnimationProperties(bool ignoreDamping = false) {
                var playable = Owner.BasePlayableComponent.Playable;
                var body = Owner.Body;
                var moveComponent = Owner.MoveComponent;

                // 歩き移動に関するパラメータ
                void UpdateDirection(string prefix, Vector3 target) {
                    var current = new Vector3();
                    current.x = playable.GetFloat($"{prefix}X");
                    current.z = playable.GetFloat($"{prefix}Z");

                    var damping = ignoreDamping ? 1.0f : 0.1f;
                    target.x = Mathf.Lerp(current.x, target.x, damping);
                    target.z = Mathf.Lerp(current.z, target.z, damping);

                    playable.SetFloat($"{prefix}X", target.x);
                    playable.SetFloat($"{prefix}Z", target.z);
                }

                if (_moveDirection.sqrMagnitude > float.Epsilon) {
                    var moveDirection = body.Transform.InverseTransformDirection(_moveDirection);
                    var rotationSpeedMultiplier = Owner.MoveComponent.GetResolver<DirectionMoveResolver>().RotationSpeedMultiplier;

                    UpdateDirection("MoveDir", moveDirection);
                    playable.SetFloat("MoveSpeedMultiplier", rotationSpeedMultiplier);
                }
                else {
                    playable.SetFloat("MoveDirX", 0.0f);
                    playable.SetFloat("MoveDirZ", 0.0f);
                    playable.SetFloat("MoveSpeedMultiplier", 0.0f);
                }

                // 接地状態に関するパラメータ
                playable.SetBool("IsAir", Owner.IsAir);

                // 走りパラメーター
                playable.SetBool("IsRun", _isRun);
                playable.SetBool("IsSprint", _isSprint);
            }
        }
    }
}