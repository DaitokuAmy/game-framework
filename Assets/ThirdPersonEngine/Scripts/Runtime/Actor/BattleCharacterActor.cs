using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// アクター基底
    /// </summary>
    public class BattleCharacterActor : CharacterActor {
        private BattleCharacterActorData _data;
        private Rigidbody _rigidbody;
        private bool _isGrounded;
        private bool _isAir;
        private float _groundHeight;

        private VelocityActorComponent _velocityComponent;

        /// <summary>地上にいるか</summary>
        public override bool IsGrounded => _isGrounded;
        /// <summary>地面の高さ</summary>
        public override float GroundHeight => _groundHeight;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActor(Body body, BattleCharacterActorData data)
            : base(body, data) {
            _data = data;
            _rigidbody = body.GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;

            _velocityComponent = AddComponent(new VelocityActorComponent(this, _data.moveActionInfo));
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            var deltaTime = Body.DeltaTime;

            // 地上状態の更新
            UpdateGroundStatus();

            // AnimationPropertyの反映
            UpdateAnimationProperties();
            
            // 移動の停止
            _velocityComponent.IsActive = CanMove();
            if (!_velocityComponent.IsActive && Component.IsMoving) {
                Component.Cancel();
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
                _velocityComponent.AddVelocity(Vector3.up * _data.jumpAction.initVelocity);
                await PlayActionAsync(_data.jumpAction.action, null, ct);
            }
        }

        /// <summary>
        /// 空中状態の更新
        /// </summary>
        private void UpdateGroundStatus() {
            var results = new RaycastHit[4];
            var groundMask = LayerMask.GetMask("Ground");
            if (Physics.RaycastNonAlloc(Body.Position + Vector3.up, Vector3.down, results, 3.0f, groundMask) > 0) {
                _isGrounded = results[0].distance <= 1.0f + _data.groundHeight;
                _isAir = results[0].distance > 1.0 + _data.airHeight;
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

            if (Component.IsMoving) {
                var moveDirection = Component.Velocity.normalized;
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