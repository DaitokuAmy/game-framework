using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ホーミング制御Projectile
    /// </summary>
    public class HomingBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("初速度")]
            public MinMaxFloat startSpeed;
            [Tooltip("加速度の最大値(0以下で無効)")]
            public MinMaxFloat maxAcceleration;
            [Tooltip("外れた後もホーミングし続けるか")]
            public bool continueAcceleration;
            [Tooltip("最大飛翔距離")]
            public float maxDistance;
            [Tooltip("着弾想定時間")]
            public MinMaxFloat duration;
            [Tooltip("想定時間をスケールさせるための基準距離(0以下なら固定時間)")]
            public float durationBaseMeter;
        }

        private readonly Vector3 _startPoint;
        private readonly float _startSpeed;
        private readonly float _maxAcceleration;
        private readonly bool _continueAcceleration;
        private readonly float _maxDistance;
        private readonly float _duration;

        private bool _stopped;
        private Vector3 _endPoint;
        private Vector3 _velocity;
        private float _distance;
        private float _timer;

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
        /// <param name="startRotation">開始向き</param>
        /// <param name="endPoint">終了座標</param>
        /// <param name="startSpeed">初速</param>
        /// <param name="duration">着弾想定時間</param>
        /// <param name="durationBaseMeter">着弾想定時間の基準距離(0以下で無効)</param>
        /// <param name="maxAcceleration">最大加速度</param>
        /// <param name="continueAcceleration">外れた後にも加速し続けるか</param>
        /// <param name="maxDistance">最大距離</param>
        public HomingBulletProjectile(Vector3 startPoint, Quaternion startRotation, Vector3 endPoint, MinMaxFloat startSpeed, MinMaxFloat maxAcceleration, bool continueAcceleration, float maxDistance, MinMaxFloat duration, float durationBaseMeter) {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _startSpeed = startSpeed.Rand();
            _duration = CalcDuration(startPoint, endPoint, duration.Rand(), durationBaseMeter);
            _maxAcceleration = maxAcceleration.Rand();
            _continueAcceleration = continueAcceleration;
            _maxDistance = maxDistance;

            Position = _startPoint;
            Rotation = startRotation;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">初期座標</param>
        /// <param name="startRotation">初期向き</param>
        /// <param name="endPoint">ターゲット座標</param>
        /// <param name="context">初期化用パラメータ</param>
        public HomingBulletProjectile(Vector3 startPoint, Quaternion startRotation, Vector3 endPoint, Context context)
            : this(startPoint, startRotation, endPoint, context.startSpeed, context.maxAcceleration, context.continueAcceleration, context.maxDistance,
                context.duration, context.durationBaseMeter) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            Position = _startPoint;
            _velocity = Rotation * Vector3.forward * _startSpeed;
            _distance = 0.0f;
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
            
            var targetVec = _endPoint - Position;
        
            // 加速度を求める
            var acceleration = Vector3.zero;

            if (_timer > 0.001f) {
                acceleration += (targetVec - _velocity * _timer) * 2.0f / (_timer * _timer);
            }
            else if (_continueAcceleration && deltaTime > 0.001f) {
                acceleration += (targetVec - _velocity * deltaTime) * 2.0f / (deltaTime * deltaTime);
            }
            
            // 加速度の最大値でクランプ
            if (_maxAcceleration > float.Epsilon) {
                acceleration = Vector3.ClampMagnitude(acceleration, _maxAcceleration);
            }

            // 速度に反映
            _velocity += acceleration * deltaTime;

            // 座標更新
            var deltaPos = _velocity * deltaTime;
            Position += deltaPos;

            // 向き更新
            if (deltaPos.sqrMagnitude > float.Epsilon) {
                Rotation = Quaternion.LookRotation(deltaPos);
            }

            // 距離更新
            _distance += deltaPos.magnitude;
            
            // Timer更新
            _timer -= deltaTime;

            return _distance < _maxDistance;
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