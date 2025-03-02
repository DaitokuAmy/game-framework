using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using UnityEngine;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// アクター基底
    /// </summary>
    public class BattleCharacterActor : CharacterActor {
        private BattleCharacterActorSetupData _setupData;
        private Rigidbody _rigidbody;
        private ActorVelocityController _velocityController;
        private bool _isGrounded;
        private bool _isAir;
        private float _groundHeight;

        /// <summary>地上にいるか</summary>
        public override bool IsGrounded => _isGrounded;
        /// <summary>地面の高さ</summary>
        public override float GroundHeight => _groundHeight;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActor(Body body, BattleCharacterActorSetupData setupData)
            : base(body, setupData) {
            _setupData = setupData;
            _rigidbody = body.GetComponent<Rigidbody>();
            _velocityController = new ActorVelocityController(this, _setupData.moveActionInfo);
            _rigidbody.useGravity = false;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _velocityController.Dispose();
            base.DisposeInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            var deltaTime = Body.DeltaTime;
            
            // 各種コントローラーの更新
            _velocityController.Update(deltaTime);

            // 地上状態の更新
            UpdateGroundStatus();

            // AnimationPropertyの反映
            UpdateAnimationProperties();
            
            // 移動の停止
            _velocityController.IsActive = CanMove();
            if (!_velocityController.IsActive && MoveController.IsMoving) {
                MoveController.Cancel();
            }
        }

        /// <summary>
        /// 位置の設定
        /// </summary>
        protected override void SetPosition(Vector3 position) {
            base.SetPosition(position);

            if (_rigidbody != null) {
                _rigidbody.MovePosition(position);
            }
        }

        /// <summary>
        /// 回転の設定
        /// </summary>
        protected override void SetRotation(Quaternion rotation) {
            base.SetRotation(rotation);

            if (_rigidbody != null) {
                _rigidbody.MoveRotation(rotation);
            }
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        public void Move(Vector2 inputDirection) {
            if (!CanMove()) {
                return;
            }
            
            // ベクトル変換
            var direction = new Vector3(inputDirection.x, 0.0f, inputDirection.y);
            direction = Body.Rotation * direction;
            DirectionMove(direction, direction.sqrMagnitude <= 0.001f ? 0.0f : 1.0f, false);
        }
        
        /// <summary>
        /// ジャンプ処理
        /// </summary>
        public async UniTask JumpAsync(CancellationToken ct) {            
            if (IsGrounded) { 
                _velocityController.AddVelocity(Vector3.up * _setupData.jumpAction.initVelocity);
                await PlayActionAsync(_setupData.jumpAction.action, null, ct);
            }
        }

        /// <summary>
        /// 空中状態の更新
        /// </summary>
        private void UpdateGroundStatus() {
            var results = new RaycastHit[4];
            var groundMask = LayerMask.GetMask("Ground");
            if (Physics.RaycastNonAlloc(Body.Position + Vector3.up, Vector3.down, results, 3.0f, groundMask) > 0) {
                _isGrounded = results[0].distance <= 1.0f + _setupData.groundHeight;
                _isAir = results[0].distance > 1.0 + _setupData.airHeight;
                _groundHeight = results[0].point.y;
            }
            else {
                _isGrounded = false;
                _isAir = true;
                _groundHeight = 0.0f;
            }
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
            }
            else {
                BasePlayableComponent.Playable.SetFloat("MoveDirX", 0.0f);
                BasePlayableComponent.Playable.SetFloat("MoveDirZ", 0.0f);
                BasePlayableComponent.Playable.SetFloat("MoveSpeedMultiplier", 0.0f);
            }

            // 接地状態に関するパラメータ
            BasePlayableComponent.Playable.SetBool("Air", _isAir);
        }

        /// <summary>
        /// 移動可能か
        /// </summary>
        private bool CanMove() {
            var stateInfo = BasePlayableComponent.Playable.GetCurrentAnimatorStateInfo(0);
            return !stateInfo.IsTag("CantMove");
        }
    }
}