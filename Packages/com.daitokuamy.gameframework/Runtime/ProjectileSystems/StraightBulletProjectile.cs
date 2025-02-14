using System;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 直進用Projectile
    /// </summary>
    [Obsolete("ShotBulletProjectileを使って下さい")]
    public class StraightBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("開始座標")]
            public Vector3 startPoint;
            [Tooltip("初速度")]
            public Vector3 startVelocity;
            [Tooltip("加速度")]
            public Vector3 acceleration;
            [Tooltip("最大距離")]
            public float maxDistance;
            [Tooltip("オブジェクトの傾き")]
            public float tilt;
        }

        private readonly Vector3 _startPoint;
        private readonly Vector3 _startVelocity;
        private readonly Vector3 _acceleration;
        private readonly float _maxDistance;
        private readonly float _tilt;

        private bool _stopped;
        private Vector3 _velocity;
        private float _distance;
        private Quaternion _tiltRotation;

        // 座標
        public Vector3 Position { get; private set; }

        // 姿勢
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="startVelocity">初速度</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="maxDistance">最大飛翔距離</param>
        /// <param name="tilt">オブジェクトの傾き</param>
        public StraightBulletProjectile(Vector3 startPoint, Vector3 startVelocity, Vector3 acceleration, float maxDistance, float tilt) {
            _startPoint = startPoint;
            _startVelocity = startVelocity;
            _acceleration = acceleration;
            _maxDistance = maxDistance;
            _tilt = tilt;

            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_startVelocity);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">初期化パラメータ</param>
        public StraightBulletProjectile(Context context)
            : this(context.startPoint, context.startVelocity, context.acceleration, context.maxDistance, context.tilt) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            _tiltRotation = Quaternion.Euler(0.0f, 0.0f, _tilt);
            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_startVelocity) * _tiltRotation;
            _velocity = _startVelocity;
            _distance = 0.0f;
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
            
            // 速度更新
            _velocity += _acceleration * deltaTime;

            // 移動量
            var deltaPos = _velocity * deltaTime;
            var restDistance = _maxDistance - _distance;
            if (deltaPos.sqrMagnitude > restDistance * restDistance) {
                deltaPos *= restDistance / deltaPos.magnitude;
            }

            // 座標更新
            Position += deltaPos;

            // 向き更新
            if (deltaPos.sqrMagnitude > float.Epsilon) {
                Rotation = Quaternion.LookRotation(deltaPos) * _tiltRotation;
            }

            // 距離更新
            _distance += _velocity.magnitude * deltaTime;

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
    }
}