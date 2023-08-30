using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// カスタムカーブ制御Projectile
    /// </summary>
    public class CustomBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("振動カーブ(-1〜1)")]
            public MinMaxAnimationCurve vibrationCurve;
            [Tooltip("振動幅")]
            public MinMaxFloat amplitude;
            [Tooltip("周波数")]
            public MinMaxFloat frequency;
            [Tooltip("奥行きカーブ(Last=1)")]
            public MinMaxAnimationCurve depthCurve;
            [Tooltip("ねじりカーブ(-1〜1)")]
            public MinMaxAnimationCurve rollCurve;
            [Tooltip("ねじりオフセット(-1～1)")]
            public MinMaxFloat rollOffset;
            [Tooltip("Vfxの傾き(角度)")]
            public MinMaxFloat tilt;
            [Tooltip("到達時間")]
            public MinMaxFloat duration;
            [Tooltip("想定時間をスケールさせるための基準距離(0以下なら固定時間)")]
            public float durationBaseMeter;
        }

        private readonly Vector3 _startPoint;
        private readonly MinMaxAnimationCurve _vibrationCurve;
        private readonly float _amplitude;
        private readonly float _frequency;
        private readonly MinMaxAnimationCurve _depthCurve;
        private readonly MinMaxAnimationCurve _rollCurve;
        private readonly float _rollOffset;
        private readonly float _tilt;
        private readonly float _duration;
        
        private Vector3 _endPoint;

        private bool _stopped;
        private float _timer;
        private Vector3 _prevPosition;

        /// <summary>現在座標</summary>
        public Vector3 Position { get; private set; }
        /// <summary>姿勢</summary>
        public Quaternion Rotation { get; private set; }
        /// <summary>目標座標</summary>
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
        /// <param name="durationBaseMeter">着弾想定時間の基準距離(0以下で無効)</param>
        public CustomBulletProjectile(Vector3 startPoint, Vector3 endPoint,
            MinMaxAnimationCurve vibrationCurve, MinMaxFloat amplitude, MinMaxFloat frequency,
            MinMaxAnimationCurve depthCurve, MinMaxAnimationCurve rollCurve, MinMaxFloat rollOffset, MinMaxFloat tilt, MinMaxFloat duration, float durationBaseMeter) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _vibrationCurve = vibrationCurve;
            _amplitude = amplitude.Rand();
            _frequency = frequency.Rand();
            _depthCurve = depthCurve;
            _rollCurve = rollCurve;
            _rollOffset = rollOffset.Rand();
            _tilt = tilt.Rand();
            _duration = duration.Rand();
            _duration = CalcDuration(startPoint, endPoint, duration.Rand(), durationBaseMeter);
            
            // Curveのランダム値を抽選
            _vibrationCurve.RandDefaultRatio();
            _depthCurve.RandDefaultRatio();
            _rollCurve.RandDefaultRatio();

            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_endPoint - _startPoint);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="endPoint">ターゲット位置</param>
        /// <param name="context">初期化パラメータ</param>
        public CustomBulletProjectile(Vector3 startPoint, Vector3 endPoint, Context context)
            : this(startPoint, endPoint, context.vibrationCurve, context.amplitude, context.frequency,
                context.depthCurve, context.rollCurve, context.rollOffset, context.tilt, context.duration, context.durationBaseMeter) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            var vector = _endPoint - _startPoint;
            Position = _startPoint;
            Rotation = Quaternion.LookRotation(vector);
            _timer = _duration;
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

            var ratio = Mathf.Clamp01(1 - _timer / _duration);

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

        /// <summary>
        /// 到達想定時間の計算
        /// </summary>
        private float CalcDuration(Vector3 start, Vector3 end, float duration, float durationBaseMeter) {
            if (durationBaseMeter <= float.Epsilon) {
                return duration;
            }
            
            var distance = (end - start).magnitude;
            return duration * (distance / durationBaseMeter);
        }
    }
}