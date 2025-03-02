using System;
using SampleGame.Infrastructure;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// アクター速度制御用クラス
    /// </summary>
    public sealed class ActorVelocityController : IDisposable {
        /// <summary>誤差</summary>
        private const float Epsilon = 0.01f;

        private readonly IActorVelocityControllerContext _context;
        
        private bool _isActive = true;
        private IMovableActor _actor;

        private Vector3 _velocity;

        /// <summary>有効状態か</summary>
        public bool IsActive {
            get => _isActive;
            set {
                if (value == _isActive) {
                    return;
                }
                
                _isActive = value;
                if (!_isActive) {
                    // 非アクティブにする際は速度をリセット
                    ResetVelocity();
                }
            }
        }
        /// <summary>重力加速度</summary>
        public float Gravity { get; set; }
        /// <summary>現在の速度</summary>
        public Vector3 Velocity => _velocity;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="actor">制御対象のActor</param>
        /// <param name="context">設定値</param>
        /// <param name="gravity">重力加速度</param>
        public ActorVelocityController(IMovableActor actor, IActorVelocityControllerContext context, float gravity = 9.8f) {
            _actor = actor;
            _context = context;
            Gravity = gravity;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _actor = null;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            if (!IsActive) {
                return;
            }

            UpdateTransform(deltaTime);
        }

        /// <summary>
        /// 速度の発生
        /// </summary>
        public void AddVelocity(Vector3 velocity) {
            _velocity += velocity;
        }

        /// <summary>
        /// 速度の設定
        /// </summary>
        public void SetVelocity(Vector3 velocity, bool ignoreY = false) {
            _velocity.x = velocity.x;
            if (!ignoreY) {
                _velocity.y = velocity.y;
            }

            _velocity.z = velocity.z;
        }

        /// <summary>
        /// 速度のリセット
        /// </summary>
        public void ResetVelocity() {
            _velocity = Vector3.zero;
        }

        /// <summary>
        /// Transformの更新
        /// </summary>
        private void UpdateTransform(float deltaTime) {
            // 地上判定
            var position = _actor.GetPosition();
            var isGrounded = _actor.IsGrounded;

            if (isGrounded && _velocity.y < 0) {
                // 下方向に移動する際、地上にいるなら速度のY軸をなくす
                _velocity.y = 0.0f;
            }
            else {
                // 重力加速度を反映
                _velocity.y -= Gravity * deltaTime;
            }
            
            // ブレーキ反映
            var brake = isGrounded ? _context.GroundBrake : _context.AirBrake;
            var velocityXZ = new Vector2(_velocity.x, _velocity.z);
            if (velocityXZ.sqrMagnitude <= brake * brake) {
                velocityXZ.x = 0.0f;
                velocityXZ.y = 0.0f;
            }
            else {
                velocityXZ -= velocityXZ.normalized * brake;
            }

            _velocity.x = velocityXZ.x;
            _velocity.z = velocityXZ.y;
            
            // 座標更新
            position += _velocity * deltaTime;
            position.y = Mathf.Max(_actor.GroundHeight, position.y);
            _actor.SetPosition(position);
        }
    }
}