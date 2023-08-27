using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 直線移動解決用クラス
    /// </summary>
    public class StraightMoveResolver : MoveResolver, IPointMoveResolver {
        private float _speed;
        private float _acceleration;
        private float _brake;
        private float _angularSpeed;
        private bool _ignoreY;

        private Vector3 _targetPosition;
        private bool _updateRotation;
        private float _currentSpeed;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="speed">移動速度</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="brake">減速度</param>
        /// <param name="angularSpeed">角速度</param>
        /// <param name="ignoreY">Y軸の移動を抑制するか</param>
        public StraightMoveResolver(float speed, float acceleration, float brake, float angularSpeed = 720.0f, bool ignoreY = true) {
            _speed = speed;
            _acceleration = acceleration;
            _brake = brake;
            _angularSpeed = angularSpeed;
            _ignoreY = ignoreY;
        }

        /// <summary>
        /// 座標指定移動
        /// </summary>
        void IPointMoveResolver.MoveToPoint(Vector3 point, bool updateRotation) {
            // ある程度の距離離れていなければ無視
            var sqrDistance = GetSqrDistance(point, _ignoreY);
            if (sqrDistance <= TolerancePow2) {
                EndMove(true);
                return;
            }
            
            // 移動開始
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

            var remainingDistance = vector.magnitude;
            var maxSpeed = _speed * speedMultiplier;
            var speed = _currentSpeed;
            
            // ブレーキ距離を求める
            var brakeDistance = maxSpeed * (maxSpeed / _brake) * 0.5f;
            
            // ブレーキ距離より近ければ速度をコントロール
            if (remainingDistance < brakeDistance) {
                // 速度がちょうど0になると到着判定がシビアになるので1割マシ
                speed = Mathf.Min(maxSpeed, (remainingDistance / brakeDistance + 0.1f) * maxSpeed);
            }
            // 加速する必要あり
            else if (_currentSpeed < maxSpeed) {
                speed = Mathf.Min(maxSpeed, speed + _acceleration * deltaTime);
            }
            
            // 到着
            var arrive = remainingDistance <= speed * deltaTime;
            if (arrive) {
                Skip();
                return;
            }
            
            var direction = vector.normalized;
            var deltaAngleY = Vector3.SignedAngle(forward, direction, Vector3.up);

            // 移動（向きが90度以上離れていたら速度を0にする）
            var speedRate = _updateRotation ? Mathf.Max(0.0f, (90 - Mathf.Abs(deltaAngleY)) / 90.0f) : 1.0f;
            var velocity = direction * (speed * speedRate);
            pos += velocity * deltaTime;
            Actor.SetPosition(pos);
            
            // 向き直し
            if (_updateRotation) {
                var angles = rot.eulerAngles;
                angles.y += Mathf.Clamp(deltaAngleY, -_angularSpeed * deltaTime, _angularSpeed * deltaTime);
                rot = Quaternion.Euler(angles);
                Actor.SetRotation(rot);
            }
        }
    }
}
