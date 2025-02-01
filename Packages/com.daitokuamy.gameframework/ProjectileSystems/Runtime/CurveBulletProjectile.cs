using System;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// カーブ制御Projectile
    /// </summary>
    [Obsolete("CustomBulletProjectileを使って下さい")]
    public class CurveBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("開始座標")]
            public Vector3 startPoint;
            [Tooltip("終了座標")]
            public Vector3 endPoint;
            [Tooltip("振動カーブ(-1〜1)")]
            public AnimationCurve vibrationCurve;
            [Tooltip("振動幅")]
            public float amplitude;
            [Tooltip("周波数")]
            public float frequency;
            [Tooltip("奥行きカーブ(Last=1)")]
            public AnimationCurve depthCurve;
            [Tooltip("ねじりカーブ(-1〜1)")]
            public AnimationCurve rollCurve;
            [Tooltip("ねじりオフセット"), Range(-1.0f, 1.0f)]
            public float rollOffset;
            [Tooltip("Vfxの傾き(角度)")]
            public float tilt;
            [Tooltip("到達時間")]
            public float duration;
            [Tooltip("到達時間が[/m]か")]
            public bool durationPerMeter;
        }

        private readonly Vector3 _startPoint;
        private Vector3 _endPoint;
        private readonly AnimationCurve _vibrationCurve;
        private readonly float _amplitude;
        private readonly float _frequency;
        private readonly AnimationCurve _depthCurve;
        private readonly AnimationCurve _rollCurve;
        private readonly float _rollOffset;
        private readonly float _tilt;
        private readonly float _duration;
        private readonly bool _durationPerMeter;

        private bool _stopped;
        private float _timer;
        private float _totalDuration;
        private Vector3 _prevPosition;

        // 座標
        public Vector3 Position { get; private set; }

        // 姿勢
        public Quaternion Rotation { get; private set; }

        // 終端座標
        public Vector3 EndPoint {
            get => _endPoint;
            set => _endPoint = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">開始座標</param>
        /// <param name="endPoint">終了座標</param>
        /// <param name="vibrationCurve">振動カーブ(-1～1)</param>
        /// <param name="amplitude">振幅</param>
        /// <param name="frequency">周波数</param>
        /// <param name="depthCurve">奥行きカーブ(1でTargetPoint)</param>
        /// <param name="rollCurve">ねじれカーブ(-1～1)</param>
        /// <param name="rollOffset">ねじれオフセット</param>
        /// <param name="tilt">傾き</param>
        /// <param name="duration">到達時間</param>
        /// <param name="durationPerMeter">到達時間指定が[/m]か</param>
        public CurveBulletProjectile(Vector3 startPoint, Vector3 endPoint, AnimationCurve vibrationCurve, float amplitude,
            float frequency,
            AnimationCurve depthCurve, AnimationCurve rollCurve, float rollOffset, float tilt, float duration,
            bool durationPerMeter) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _vibrationCurve = vibrationCurve;
            _amplitude = amplitude;
            _frequency = frequency;
            _depthCurve = depthCurve;
            _rollCurve = rollCurve;
            _rollOffset = rollOffset;
            _tilt = tilt;
            _duration = duration;
            _durationPerMeter = durationPerMeter;

            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_endPoint - _startPoint);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">初期化パラメータ</param>
        public CurveBulletProjectile(Context context)
            : this(context.startPoint, context.endPoint, context.vibrationCurve, context.amplitude, context.frequency,
                context.depthCurve,
                context.rollCurve, context.rollOffset, context.tilt, context.duration, context.durationPerMeter) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            var vector = _endPoint - _startPoint;
            Position = _startPoint;
            Rotation = Quaternion.LookRotation(vector);
            if (_durationPerMeter) {
                var distance = vector.magnitude;
                _totalDuration = _duration * distance;
                _timer = _totalDuration;
            }
            else {
                _totalDuration = _duration;
                _timer = _duration;
            }

            _prevPosition = _startPoint;
            _stopped = false;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool IProjectile.Update(float deltaTime) {
            if (_stopped) {
                return false;
            }
            
            var vector = _endPoint - _startPoint;
            var distance = vector.magnitude;

            if (distance <= float.Epsilon) {
                Position = _endPoint;
                Rotation = Quaternion.identity;
                return true;
            }

            // 時間更新
            _timer -= deltaTime;

            var ratio = Mathf.Clamp01(1 - _timer / _totalDuration);

            // 位置計算
            var vibration = _vibrationCurve.Evaluate(ratio * _frequency % 1.0f) * _amplitude;
            var depth = _depthCurve.Evaluate(ratio);
            var roll = _rollCurve.Evaluate(ratio) + _rollOffset;
            var forward = vector.normalized;
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var up = Vector3.Cross(forward, right);
            var relativePos = Vector3.zero;
            var radian = roll * Mathf.PI;
            relativePos += vector * depth;
            relativePos += up * (Mathf.Cos(radian) * vibration);
            relativePos += right * (Mathf.Sin(radian) * vibration);
            Position = _startPoint + relativePos;
            Rotation = Quaternion.LookRotation(Position - _prevPosition, up) * Quaternion.Euler(0.0f, 0.0f, _tilt);
            _prevPosition = Position;

            return _timer > 0.0f;
        }

        /// <summary>
        /// 飛翔終了
        /// </summary>
        void IProjectile.Stop(Vector3? stopPosition) {
            if (stopPosition != null) {
                Position = stopPosition.Value;
            }
            
            _stopped = true;
        }
    }
}