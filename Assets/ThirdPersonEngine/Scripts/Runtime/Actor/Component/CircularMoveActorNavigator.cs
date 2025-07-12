using UnityEngine;
using UnityEngine.Splines;

namespace ThirdPersonEngine {
    /// <summary>
    /// 回り込み移動用のActorNavigator
    /// </summary>
    public class CircularMoveActorNavigator : IActorNavigator {
        private readonly Vector3 _centerPoint;
        private readonly float _targetAngle;
        private readonly float _targetDistance;

        private Spline _spline;
        private Vector3 _goalPoint;
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="centerPoint">基準座標</param>
        /// <param name="targetAngle">回り込み角度(正面が0度)</param>
        /// <param name="targetDistance">回り込み後の距離</param>
        public CircularMoveActorNavigator(Vector3 centerPoint, float targetAngle, float targetDistance) {
            _centerPoint = centerPoint;
            _targetAngle = targetAngle;
            _targetDistance = targetDistance;
        }

        /// <summary>
        /// 移動開始処理
        /// </summary>
        /// <param name="currentPosition">現在座標</param>
        /// <param name="speed">移動速度</param>
        void IActorNavigator.Start(Vector3 currentPosition, float speed) {
            _position = currentPosition;
            _speed = speed;
            
            // Splineの作成
            _spline = CreateSpline(currentPosition, _centerPoint, _targetAngle, _targetDistance);
            _goalPoint = _spline.EvaluatePosition(1.0f);
            
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
            var deltaDistance = _speed / 10.0f;

            // 移動想定距離よりも最近点が遠い場合は、そこに向かう
            if (distance >= deltaDistance) {
                _steeringTarget = nearest;
            }
            // そうでない場合は、余った距離を進めた位置に向かう
            else {
                var relativeDistance = deltaDistance - distance;
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

        /// <summary>
        /// Splineの作成
        /// </summary>
        /// <param name="currentPoint">現在座標</param>
        /// <param name="centerPoint">回り込み基準座標</param>
        /// <param name="targetAngle">回り込み角度(正面0度)</param>
        /// <param name="targetDistance">回り込み距離</param>
        private Spline CreateSpline(Vector3 currentPoint, Vector3 centerPoint, float targetAngle, float targetDistance) {
            targetAngle = Mathf.Repeat(targetAngle + 180.0f, 360.0f) - 180.0f;

            // ベース座標から見た正面と距離を求める
            var direction = currentPoint - centerPoint;
            var currentDistance = direction.magnitude;
            direction.Normalize();
            
            // 角度差に応じて分解数を決める
            var diffAngle = Mathf.Abs(targetAngle);
            var knotCount = Mathf.FloorToInt(diffAngle / 15.0f) + 2;
            
            // SplineのKnotを作成
            var knots = new BezierKnot[knotCount];
            var angleUnit = targetAngle / (knotCount - 1);
            var rotUnit = Quaternion.Euler(0.0f, angleUnit, 0.0f);
            var knotDirection = direction;
            var distanceRatio = targetDistance / currentDistance;
            for (var i = 0; i < knotCount; i++) {
                var t = i / (float)(knotCount - 1);
                var distanceT = distanceRatio > 1.0f ? t * t : 1 - (1 - t) * (1 - t);
                var distance = Mathf.Lerp(currentDistance, targetDistance,  distanceT);
                var point = centerPoint + knotDirection * distance;
                knots[i] = new BezierKnot(point);
                knotDirection = rotUnit * knotDirection;
            }
            
            var spline = new Spline(knots);
            var tension = 1.0f;
            for (var i = 0; i < knotCount; i++) {
                spline.SetAutoSmoothTension(i, tension);
            }

            return spline;
        }
    }
}
