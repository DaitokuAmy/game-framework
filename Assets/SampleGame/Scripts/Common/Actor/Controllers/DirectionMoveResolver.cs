using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 向き移動解決用クラス
    /// </summary>
    public class DirectionMoveResolver : MoveResolver, IDirectionMoveResolver {
        private readonly float _speed;
        private readonly float _angularSpeed;
        private readonly bool _ignoreY;

        private Vector3 _direction;
        private bool _updateRotation;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="speed">移動速度</param>
        /// <param name="angularSpeed">角速度</param>
        /// <param name="ignoreY">Y軸の移動を抑制するか</param>
        public DirectionMoveResolver(float speed, float angularSpeed = 720.0f, bool ignoreY = true) {
            _speed = speed;
            _angularSpeed = angularSpeed;
            _ignoreY = ignoreY;
        }

        /// <summary>
        /// 座標指定移動
        /// </summary>
        void IDirectionMoveResolver.MoveDirection(Vector3 direction, bool updateRotation) {
            // 移動向きがなければ移動しない
            var sqrDistance = direction.sqrMagnitude;
            if (sqrDistance <= TolerancePow2) {
                EndMove(true);
                return;
            }
            
            // 移動情報格納
            _direction = direction;
            _updateRotation = updateRotation;
            StartMove();
        }

        /// <summary>
        /// 移動スキップ
        /// </summary>
        protected override void SkipInternal() {
            _direction = Vector3.zero;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime, float speedMultiplier) {
            var pos = Actor.GetPosition();
            var rot = Actor.GetRotation();
            var forward = rot * Vector3.forward;
            
            var vector = _direction;
            if (_ignoreY) {
                vector.y = 0.0f;
            }

            // 移動しない状態になっている
            if (vector.sqrMagnitude <= TolerancePow2) {
                EndMove(true);
            }

            var speed = _speed * speedMultiplier;
            
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
            
            // 移動向きを初期化
            _direction = Vector3.zero;
        }
    }
}
