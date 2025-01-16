using SampleGame.Infrastructure;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 直線移動解決用クラス
    /// </summary>
    public class StraightMoveResolver : MoveResolver, IPointMoveResolver {
        private IMoveResolverContext _context;
        private bool _ignoreY;

        private float _arrivedDistance;
        private float _currentSpeed;
        private Vector3 _targetPosition;
        private bool _updateRotation;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">設定値</param>
        /// <param name="ignoreY">Y軸の移動を抑制するか</param>
        public StraightMoveResolver(IMoveResolverContext context, bool ignoreY = true) {
            _context = context;
            _ignoreY = ignoreY;
        }

        /// <summary>
        /// 座標指定移動
        /// </summary>
        void IPointMoveResolver.MoveToPoint(Vector3 point, float speedMultiplier, float arrivedDistance, bool updateRotation) {
            
            // ある程度の距離離れていなければ無視
            var sqrDistance = GetSqrDistance(point, _ignoreY);
            if (sqrDistance <= (arrivedDistance + Tolerance) * (arrivedDistance + Tolerance)) {
                EndMove(true);
                return;
            }
            
            // 移動開始
            _arrivedDistance = arrivedDistance;
            _targetPosition = point;
            _updateRotation = updateRotation;
            _currentSpeed = 0.0f;
            StartMove();
        }

        /// <summary>
        /// 移動スキップ
        /// </summary>
        protected override void SkipInternal() {
            var point = _targetPosition;
            if (_ignoreY) {
                point.y = Actor.GetPosition().y;
            }

            Actor.SetPosition(point);
            _currentSpeed = 0.0f;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime, float speedMultiplier) {
            var pos = Actor.GetPosition();
            var rot = Actor.GetRotation();
            var forward = rot * Vector3.forward;
            
            var vector = _targetPosition - pos;
            if (_ignoreY) {
                vector.y = 0.0f;
            }

            var remainingDistance = vector.magnitude - _arrivedDistance;
            var maxSpeed = _context.MaxSpeed * speedMultiplier;
            var speed = maxSpeed;
            
            // ブレーキにかかる距離を計算
            var brakeDistance = maxSpeed * (maxSpeed / _context.Brake) * 0.5f;
            
            // 残り距離がブレーキ予定距離より短い場合は、速度をコントロールする
            if (remainingDistance < brakeDistance) {
                speed = Mathf.Min(maxSpeed, speed * remainingDistance / brakeDistance + maxSpeed * 0.1f);
            }
            // 現在速度が最高速度に到達していない場合、加速させる
            else if (_currentSpeed < maxSpeed) {
                speed = Mathf.Min(maxSpeed, _currentSpeed + _context.Acceleration * deltaTime);
            }

            _currentSpeed = speed;
            
            var direction = vector.normalized;
            var velocity = direction * speed;
            
            // 到着
            var arrive = remainingDistance <= speed * deltaTime;
            if (arrive) {
                pos += velocity * deltaTime;
                Actor.SetPosition(pos);
                EndMove(true);
                return;
            }
            
            var deltaAngleY = Vector3.SignedAngle(forward, direction, Vector3.up);

            // 移動（向きが90度以上離れていたら速度を0にする）
            var speedRate = _updateRotation ? Mathf.Max(0.0f, (90 - Mathf.Abs(deltaAngleY)) / 90.0f) : 1.0f;
            pos += velocity * (speedRate * deltaTime);
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
