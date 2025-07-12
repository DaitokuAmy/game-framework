using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace ThirdPersonEngine {
    /// <summary>
    /// 座標配列を使ったActorNavigator
    /// </summary>
    public class PointArrayActorNavigator : IActorNavigator {
        private readonly Spline _spline;
        private readonly bool _reverse;
        private readonly Vector3 _goalPoint;

        private Vector3 _position;
        private float _speed;
        private Vector3 _steeringTarget;
        private float _remainingDistance;

        /// <summary>有効か</summary>
        bool IActorNavigator.IsValid => _spline != null && _spline.Count > 0;
        /// <summary>移動先座標</summary>
        Vector3 IActorNavigator.SteeringTarget => _steeringTarget;
        /// <summary>目的地</summary>
        Vector3 IActorNavigator.GoalPoint => _spline?.EvaluatePosition(1.0f) ?? Vector3.zero;
        /// <summary>目的地までの残り距離</summary>
        float IActorNavigator.RemainingDistance => _remainingDistance;

        /// <summary>タイムアウト時間</summary>
        public float TimeOutDuration { get; private set; } = -1.0f;

        /// <summary>目的方向のT値</summary>
        private float GoalT => _reverse ? 0.0f : 1.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="corners">ルートを示す座標配列</param>
        /// <param name="reverse">反対に移動するか</param>
        public PointArrayActorNavigator(Vector3[] corners, bool reverse = false) {
            _spline = new Spline(corners.Select(x => {
                x.y = 0.0f;
                return new BezierKnot(x);
            }));
            _reverse = reverse;
            _goalPoint = _spline.EvaluatePosition(GoalT);
        }

        /// <summary>
        /// 移動開始処理
        /// </summary>
        /// <param name="currentPosition">現在座標</param>
        /// <param name="speed">移動速度</param>
        void IActorNavigator.Start(Vector3 currentPosition, float speed) {
            _position = currentPosition;
            _speed = speed;
            
            // タイムアウト時間を設定
            TimeOutDuration = _spline.GetLength() / _speed * 1.5f;
            
            // 目的座標の更新
            UpdateSteeringTarget();
            // 目的地までの距離の更新
            UpdateRemainingDistance();
        }

        /// <summary>
        /// 移動先座標の設定
        /// </summary>
        /// <param name="position">移動先座標</param>
        void IActorNavigator.SetNextPosition(Vector3 position) {
            _position = position;
            
            // 目的座標の更新
            UpdateSteeringTarget();
            // 目的地までの距離の更新
            UpdateRemainingDistance();
        }

        /// <summary>
        /// 移動先座標の更新
        /// </summary>
        private void UpdateSteeringTarget() {
            // 現在座標に最も近いポイントを取得
            SplineUtility.GetNearestPoint(_spline, _position, out var nearest, out var nearestT);
            
            // NavMeshはY軸を見ない
            nearest.y = _position.y;
            var distance = ((Vector3)nearest - _position).magnitude;
            
            // 移動速度を元に擬似的に目的座標を計算
            var deltaDistance = _speed / 5.0f;

            // 移動想定距離よりも最近点が遠い場合は、そこに向かう
            if (distance >= deltaDistance) {
                _steeringTarget = nearest;
            }
            // そうでない場合は、余った距離を進めた位置に向かう
            else {
                var relativeDistance = deltaDistance - distance;
                relativeDistance = _reverse ? -relativeDistance : relativeDistance;
                _steeringTarget = _spline.GetPointAtLinearDistance(Mathf.Clamp01(nearestT), relativeDistance, out _);
                _steeringTarget.y = _position.y;
            }
        }

        /// <summary>
        /// 目的地までの距離の更新
        /// </summary>
        private void UpdateRemainingDistance() {
            var vector = _goalPoint - _position;
            vector.y = 0.0f;
            _remainingDistance = vector.magnitude;
        }
    }
}
