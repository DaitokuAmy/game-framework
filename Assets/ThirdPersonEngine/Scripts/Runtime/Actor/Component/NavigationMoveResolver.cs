using ThirdPersonEngine;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// ナビゲーション移動解決用クラス
    /// </summary>
    public class NavigationMoveResolver : MoveResolver, INavigationMoveResolver {
        private IActorNavigator _navigator;

        private readonly IMoveResolverContext _context;
        private readonly bool _ignoreY;

        private float _arrivedDistance;
        private float _currentSpeed;
        private bool _updateRotation;
        private float _timer;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">設定値</param>
        /// <param name="ignoreY">Y軸の移動を抑制するか</param>
        public NavigationMoveResolver(IMoveResolverContext context, bool ignoreY = true) {
            _context = context;
            _ignoreY = ignoreY;
            _timer = -1.0f;
        }

        /// <summary>
        /// ナビゲーション移動
        /// </summary>
        void INavigationMoveResolver.Move(IActorNavigator navigator, float speedMultiplier, float arrivedDistance, bool updateRotation) {
            // Navigatorがいなければ何もしない
            if (navigator == null) {
                _navigator = null;
                EndMove(false);
                return;
            }

            _navigator = navigator;
            _timer = _navigator.TimeOutDuration;
            
            // 移動開始
            _navigator.Start(Actor.GetPosition(), _context.MaxSpeed * speedMultiplier);
            _arrivedDistance = arrivedDistance;
            _updateRotation = updateRotation;
            _currentSpeed = 0.0f;
            StartMove();
        }

        /// <summary>
        /// 移動スキップ
        /// </summary>
        protected override void SkipInternal() {
            if (_navigator == null) {
                return;
            }
            
            var point = _navigator.GoalPoint;
            if (_ignoreY) {
                point.y = Actor.GetPosition().y;
            }

            Actor.SetPosition(point);
            _currentSpeed = 0.0f;
            _timer = -1.0f;
            _navigator = null;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime, float speedMultiplier) {
            if (_navigator == null || !_navigator.IsValid) {
                Cancel();
                return;
            }
            
            // タイムアウトチェック
            if (_timer >= 0.0f && _timer - deltaTime < 0.0f) {
                Cancel();
                return;
            }
            
            var pos = Actor.GetPosition();
            var rot = Actor.GetRotation();
            var forward = rot * Vector3.forward;
            
            var vector = _navigator.SteeringTarget - pos;
            if (_ignoreY) {
                vector.y = 0.0f;
            }

            var remainingDistance = _navigator.RemainingDistance - _arrivedDistance;
            var maxSpeed = _context.MaxSpeed * speedMultiplier;
            var speed = maxSpeed;
            
            // ブレーキにかかる距離を計算
            var brakeDistance = maxSpeed * (maxSpeed / _context.Brake) * 0.5f;
            
            // 残り距離がブレーキ予定距離より短い場合は、速度をコントロールする
            if (remainingDistance < brakeDistance) {
                speed = Mathf.Max(0.0f, Mathf.Min(maxSpeed, speed * remainingDistance / brakeDistance + maxSpeed * 0.1f));
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
            
            // NavMeshAgentへの情報更新
            _navigator.SetNextPosition(pos);

            // タイマー更新
            if (_timer >= 0.0f) {
                _timer -= deltaTime;
            }
        }
    }
}
