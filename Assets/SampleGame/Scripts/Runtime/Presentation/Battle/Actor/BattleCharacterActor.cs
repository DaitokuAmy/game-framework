using GameFramework.BodySystems;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// アクター基底
    /// </summary>
    public class BattleCharacterActor : CharacterActor {
        private BattleCharacterActorSetupData _setupData;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActor(Body body, BattleCharacterActorSetupData setupData)
            : base(body, setupData) {
            _setupData = setupData;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            // AnimationPropertyの反映
            UpdateAnimationProperties();
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        public void Move(Vector2 inputDirection) {
            // ベクトル変換
            var direction = new Vector3(inputDirection.x, 0.0f, inputDirection.y);
            direction = Body.Rotation * direction;
            DirectionMove(direction, 1.0f, false);
        }

        /// <summary>
        /// アニメーション用プロパティの反映
        /// </summary>
        private void UpdateAnimationProperties(bool ignoreDamping = false) {
            // 歩き移動に関するパラメータ
            void UpdateDirection(string prefix, Vector3 target) {
                var current = new Vector3();
                current.x = BasePlayableComponent.Playable.GetFloat($"{prefix}X");
                current.z = BasePlayableComponent.Playable.GetFloat($"{prefix}Z");

                var damping = ignoreDamping ? 1.0f : 0.1f;
                target.x = Mathf.Lerp(current.x, target.x, damping);
                target.z = Mathf.Lerp(current.z, target.z, damping);

                BasePlayableComponent.Playable.SetFloat($"{prefix}X", target.x);
                BasePlayableComponent.Playable.SetFloat($"{prefix}Z", target.z);
            }

            if (MoveController.IsMoving) {
                var moveDirection = MoveController.Velocity.normalized;
                moveDirection = Body.Transform.InverseTransformDirection(moveDirection);

                UpdateDirection("MoveDir", moveDirection);
                BasePlayableComponent.Playable.SetFloat("Speed", MoveController.SpeedMultiplier);
            }
            else {
                BasePlayableComponent.Playable.SetFloat("MoveDirX", 0.0f);
                BasePlayableComponent.Playable.SetFloat("MoveDirZ", 0.0f);
                BasePlayableComponent.Playable.SetFloat("Speed", 0.0f);
            }
        }
    }
}