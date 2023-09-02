using System;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Serialization;

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
            [FormerlySerializedAs("rollCurve")]
            [Tooltip("傾きカーブ(-1〜1)")]
            public MinMaxAnimationCurve tiltCurve;
            [FormerlySerializedAs("rollOffset")]
            [Tooltip("傾きオフセット(-1～1)")]
            public MinMaxFloat tiltOffset;
            [Tooltip("進捗する時間軸カーブ")]
            public MinMaxAnimationCurve timeCurve;
            [FormerlySerializedAs("tilt")]
            [Tooltip("オブジェクトの傾き(角度)")]
            public MinMaxFloat roll;
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
        private readonly MinMaxAnimationCurve _tiltCurve;
        private readonly float _tiltOffset;
        private readonly MinMaxAnimationCurve _timeCurve;
        private readonly Quaternion _roll;
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
        /// <param name="tiltCurve">ねじれカーブ(-1～1)</param>
        /// <param name="tiltOffset">ねじれオフセット</param>
        /// <param name="timeCurve">時間軸カーブ</param>
        /// <param name="roll">傾き</param>
        /// <param name="duration">到達時間</param>
        /// <param name="durationBaseMeter">着弾想定時間の基準距離(0以下で無効)</param>
        public CustomBulletProjectile(Vector3 startPoint, Vector3 endPoint,
            MinMaxAnimationCurve vibrationCurve, MinMaxFloat amplitude, MinMaxFloat frequency,
            MinMaxAnimationCurve depthCurve, MinMaxAnimationCurve tiltCurve, MinMaxFloat tiltOffset, MinMaxAnimationCurve timeCurve, MinMaxFloat roll, MinMaxFloat duration, float durationBaseMeter) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _vibrationCurve = vibrationCurve;
            _amplitude = amplitude.Rand();
            _frequency = frequency.Rand();
            _depthCurve = depthCurve;
            _tiltCurve = tiltCurve;
            _tiltOffset = tiltOffset.Rand();
            _timeCurve = timeCurve;
            _roll = Quaternion.Euler(0.0f, 0.0f, roll.Rand());
            _duration = duration.Rand();
            _duration = CalcDuration(startPoint, endPoint, duration.Rand(), durationBaseMeter);
            
            // Curveのランダム値を抽選
            _vibrationCurve.RandDefaultRatio();
            _depthCurve.RandDefaultRatio();
            _tiltCurve.RandDefaultRatio();
            _timeCurve.RandDefaultRatio();

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
                context.depthCurve, context.tiltCurve, context.tiltOffset, context.timeCurve, context.roll, context.duration, context.durationBaseMeter) {
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
            
            // 時間軸カーブ適用
            if (_timeCurve.minValue.keys.Length >= 2) {
                ratio = Mathf.Clamp01(_timeCurve.Evaluate(ratio));
            }

            // 位置計算
            var vibration = _vibrationCurve.Evaluate(ratio * _frequency % 1.0f) * _amplitude;
            var depth = _depthCurve.Evaluate(ratio);
            var roll = _tiltCurve.Evaluate(ratio) + _tiltOffset;
            var forward = vector.normalized;
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var up = Vector3.Cross(forward, right);
            var relativePos = Vector3.zero;
            var radian = roll * Mathf.PI;
            relativePos += vector * depth;
            relativePos += up * (Mathf.Cos(radian) * vibration);
            relativePos += right * (Mathf.Sin(radian) * vibration);
            Position = _startPoint + relativePos;
            Rotation = Quaternion.LookRotation(Position - _prevPosition, up) * _roll;
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