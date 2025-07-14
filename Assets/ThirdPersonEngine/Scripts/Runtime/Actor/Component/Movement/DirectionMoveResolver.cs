using ThirdPersonEngine;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 向き移動解決用クラス
    /// </summary>
    public class DirectionMoveResolver : MoveResolver, IDirectionMoveResolver {
        private readonly IMoveResolverContext _context;
        private readonly bool _ignoreY;

        private float _currentSpeed;
        private Vector3 _direction;
        private bool _updateRotation;
        
        /// <summary>回転による速度係数</summary>
        public float RotationSpeedMultiplier { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">設定値</param>
        /// <param name="ignoreY">Y軸の移動を抑制するか</param>
        public DirectionMoveResolver(IMoveResolverContext context, bool ignoreY = true) {
            _context = context;
            _ignoreY = ignoreY;
        }
        
        /// <summary>
        /// 向き指定移動(移動し続ける)
        /// </summary>
        /// <param name="direction">移動向き</param>
        /// <param name="speedMultiplier">速度係数</param>
        /// <param name="updateRotation">回転制御を行うか</param>
        void IDirectionMoveResolver.MoveToDirection(Vector3 direction, float speedMultiplier, bool updateRotation) {
            // 移動開始
            _direction = direction.sqrMagnitude <= 0.001f ? Vector3.zero : direction.normalized;
            _updateRotation = updateRotation;
            StartMove();

            // 移動がなければ終わる
            if (_direction.sqrMagnitude <= 0.001f) {
                EndMove(true);
            }
        }

        /// <summary>
        /// 移動スキップ
        /// </summary>
        protected override void SkipInternal() {
            // 何もしない
        }

        /// <summary>
        /// 移動キャンセル
        /// </summary>
        protected override void CancelInternal() {
            // 移動を止める
            _currentSpeed = 0.0f;
            _direction = Vector3.zero;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime, float speedMultiplier) {
            // 移動向きがなければ終わる
            if (_direction.sqrMagnitude <= 0.001f) {
                EndMove(true);
                return;
            }
            
            var pos = Actor.GetPosition();
            var rot = Actor.GetRotation();
            var forward = rot * Vector3.forward;
            
            var vector = _direction;
            if (_ignoreY) {
                vector.y = 0.0f;
            }
            
            var maxSpeed = _context.MaxSpeed * speedMultiplier;
            var speed = maxSpeed;
            
            // 現在速度が最高速度に到達していない場合、加速させる
            if (_currentSpeed < maxSpeed) {
                speed = Mathf.Min(maxSpeed, _currentSpeed + _context.Acceleration * deltaTime);
            }

            _currentSpeed = speed;
            
            var direction = vector.normalized;
            var velocity = direction * speed;
            
            var deltaAngleY = Vector3.SignedAngle(forward, direction, Vector3.up);

            // 移動（向きが90度以上離れていたら速度を0にする）
            RotationSpeedMultiplier = _updateRotation ? Mathf.Max(0.0f, (90 - Mathf.Abs(deltaAngleY)) / 90.0f) : 1.0f;
            pos += velocity * (RotationSpeedMultiplier * deltaTime);
            Actor.SetPosition(pos);
            
            // 向き直し
            if (_updateRotation) {
                var angles = rot.eulerAngles;
                angles.y += Mathf.Clamp(deltaAngleY, -_context.AngularSpeed * deltaTime, _context.AngularSpeed * deltaTime);
                rot = Quaternion.Euler(angles);
                Actor.SetRotation(rot);
            }
        }
    }
}
