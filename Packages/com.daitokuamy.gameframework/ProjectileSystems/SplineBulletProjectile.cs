#if USE_SPLINES
using System;
using GameFramework.Core;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// スプライン制御Projectile
    /// </summary>
    public class SplineBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("スプラインカーブを入れたPrefab")]
            public SplineContainer splinePrefab;
            [Tooltip("進捗する時間軸カーブ")]
            public MinMaxAnimationCurve timeCurve;
            [FormerlySerializedAs("_roll")]
            [Tooltip("スプラインの傾き(角度)")]
            public MinMaxFloat tilt;
            [Tooltip("オブジェクトの傾き(角度)")]
            public MinMaxFloat roll;
            [Tooltip("到達時間")]
            public MinMaxFloat duration;
            [Tooltip("想定時間をスケールさせるための基準距離(0以下なら固定時間)")]
            public float durationBaseMeter;
        }

        private readonly Vector3 _startPoint;
        private readonly SplineContainer _splinePrefab;
        private readonly MinMaxAnimationCurve _timeCurve;
        private readonly Quaternion _tilt;
        private readonly Quaternion _roll;
        private readonly float _duration;

        private Vector3 _endPoint;

        private float _splineDistance;
        private bool _stopped;
        private float _timer;

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
        /// <param name="splinePrefab">スプラインを仕込んだPrefab</param>
        /// <param name="timeCurve">時間軸カーブ</param>
        /// <param name="tilt">曲線の傾き</param>
        /// <param name="roll">オブジェクトの回転</param>
        /// <param name="duration">到達時間</param>
        /// <param name="durationBaseMeter">着弾想定時間の基準距離(0以下で無効)</param>
        public SplineBulletProjectile(Vector3 startPoint, Vector3 endPoint,
            SplineContainer splinePrefab, MinMaxAnimationCurve timeCurve, MinMaxFloat tilt, MinMaxFloat roll, MinMaxFloat duration, float durationBaseMeter) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _splinePrefab = splinePrefab;
            _timeCurve = timeCurve;
            _tilt = Quaternion.Euler(0.0f, 0.0f, tilt.Rand());
            _roll = Quaternion.Euler(0.0f, 0.0f, roll.Rand());
            _duration = duration.Rand();
            _duration = CalcDuration(startPoint, endPoint, duration.Rand(), durationBaseMeter);
            
            // カーブの乱数初期化
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
        public SplineBulletProjectile(Vector3 startPoint, Vector3 endPoint, Context context)
            : this(startPoint, endPoint, context.splinePrefab, context.timeCurve, context.tilt, context.roll, context.duration, context.durationBaseMeter) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            var vector = _endPoint - _startPoint;
            Position = _startPoint;
            Rotation = Quaternion.LookRotation(vector);
            _splineDistance = _splinePrefab.Spline.EvaluatePosition(1.0f).z;
            _timer = _duration;
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

            // ローカル位置を取得
            _splinePrefab.Spline.Evaluate(ratio, out var localPosition, out var localTangent, out var localUpVector);

            // 進行方向に対する空間に変換
            var pivot = _startPoint;
            var scale = distance / _splineDistance;
            var rotation = Quaternion.FromToRotation(Vector3.forward, vector) * _tilt;
            var position = rotation * (localPosition * new float3(1, 1, scale)) + pivot;
            var tangent = rotation * (localTangent * new float3(1, 1, scale));
            var upVector = rotation * (localUpVector * new float3(1, 1, scale));

            Position = position;
            Rotation = Quaternion.LookRotation(tangent, upVector) * _roll;

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
#endif