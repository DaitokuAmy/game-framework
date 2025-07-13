using UnityEngine;
using UnityEngine.Splines;

namespace ThirdPersonEngine {
    /// <summary>
    /// Splineを使ったActorNavigator
    /// </summary>
    public class SplineActorNavigator : IActorNavigator {
        private readonly Spline _spline;
        private readonly bool _worldSpace;
        private readonly Transform _container;
        private readonly Vector3 _basePosition;
        private readonly Quaternion _baseRotation;
        private readonly Vector3 _goalPoint;
        private readonly bool _reverse;

        private float _speed;
        private Vector3 _position;
        private Vector3 _steeringTarget;
        private float _remainingDistance;

        /// <summary>有効か</summary>
        bool IActorNavigator.IsValid => _spline != null;
        /// <summary>移動先座標</summary>
        Vector3 IActorNavigator.SteeringTarget => _steeringTarget;
        /// <summary>目的地</summary>
        Vector3 IActorNavigator.GoalPoint => _goalPoint;
        /// <summary>目的地までの残り距離</summary>
        float IActorNavigator.RemainingDistance => _remainingDistance;

        /// <summary>タイムアウト時間</summary>
        public float TimeOutDuration { get; private set; } = -1.0f;
        
        /// <summary>目的方向のT値</summary>
        private float GoalT => _reverse ? 0.0f : 1.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="spline">移動パス</param>
        /// <param name="basePosition">パスを座標変換するための基準位置</param>
        /// <param name="baseRotation">パスを座標変換するための基準向き</param>
        /// <param name="reverse">逆再生するか</param>
        public SplineActorNavigator(Spline spline, Vector3 basePosition, Quaternion baseRotation, bool reverse = false) {
            _spline = spline;
            _basePosition = basePosition;
            _baseRotation = baseRotation;
            _worldSpace = false;
            _reverse = reverse;
            _goalPoint = TransformPoint(_spline.EvaluatePosition(GoalT));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="spline">移動パス</param>
        /// <param name="container">パスが所属しているTransform</param>
        /// <param name="reverse">逆再生するか</param>
        public SplineActorNavigator(Spline spline, Transform container = null, bool reverse = false) {
            _spline = spline;
            _container = container;
            _worldSpace = container == null;
            _reverse = reverse;
            _goalPoint = TransformPoint(_spline.EvaluatePosition(GoalT));
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="splineContainer">移動パスを含むコンテナ</param>
        /// <param name="reverse">逆再生するか</param>
        public SplineActorNavigator(SplineContainer splineContainer, bool reverse = false)
            : this(splineContainer.Spline, splineContainer.transform, reverse) {
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
            var localPoint = InverseTransformPoint(_position);
            
            // 現在座標に最も近いポイントを取得
            var distance = SplineUtility.GetNearestPoint(_spline, localPoint, out var nearest, out var nearestT);
            
            // 移動速度を元に擬似的に目的座標を計算
            var deltaDistance = _speed / 30.0f;

            // 移動想定距離よりも最近点が遠い場合は、そこに向かう
            if (distance >= deltaDistance) {
                _steeringTarget = nearest;
            }
            // そうでない場合は、余った距離を進めた位置に向かう
            else {
                var relativeDistance = deltaDistance - distance;
                relativeDistance = _reverse ? -relativeDistance : relativeDistance;
                _steeringTarget = _spline.GetPointAtLinearDistance(Mathf.Clamp01(nearestT), relativeDistance, out _);
            }
            
            // Local > World変換
            _steeringTarget = TransformPoint(_steeringTarget);
        }

        /// <summary>
        /// 目的地までの距離の更新
        /// </summary>
        private void UpdateRemainingDistance() {
            _remainingDistance = (_goalPoint - _position).magnitude;
        }

        /// <summary>
        /// 座標変換
        /// </summary>
        private Vector3 TransformPoint(Vector3 localPosition) {
            if (_worldSpace) {
                return localPosition;
            }

            if (_container != null) {
                return _container.TransformPoint(localPosition);
            }

            return _baseRotation * localPosition + _basePosition;
        }

        /// <summary>
        /// 座標逆変換
        /// </summary>
        private Vector3 InverseTransformPoint(Vector3 worldPosition) {
            if (_worldSpace) {
                return worldPosition;
            }

            if (_container != null) {
                return _container.InverseTransformPoint(worldPosition);
            }

            return Quaternion.Inverse(_baseRotation) * (worldPosition - _basePosition);
        }
    }
}
