using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 射出用Projectile
    /// </summary>
    public class ShotBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("初速度")]
            public MinMaxFloat startSpeed;
            [Tooltip("加速度")]
            public MinMaxFloat acceleration;
            [Tooltip("重力加速度")]
            public float gravity;
            [Tooltip("最大距離")]
            public float maxDistance;
            [Tooltip("オブジェクトの傾き(角度)")]
            public MinMaxFloat roll;
        }

        private readonly Vector3 _startPoint;
        private readonly Quaternion _startRotation;
        private readonly float _startSpeed;
        private readonly float _acceleration;
        private readonly float _gravity;
        private readonly float _maxDistance;
        private readonly float _roll;

        private bool _stopped;
        private Vector3 _velocity;
        private float _distance;
        private Quaternion _rollRotation;

        // 座標
        public Vector3 Position { get; private set; }

        // 姿勢
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="startRotation">向き</param>
        /// <param name="startSpeed">初速</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="gravity">重力加速度</param>
        /// <param name="maxDistance">最大飛翔距離</param>
        /// <param name="tilt">オブジェクトの傾き</param>
        public ShotBulletProjectile(Vector3 startPoint, Quaternion startRotation, MinMaxFloat startSpeed, MinMaxFloat acceleration, float gravity, float maxDistance, MinMaxFloat roll) {
            _startPoint = startPoint;
            _startRotation = startRotation;
            _startSpeed = startSpeed.Rand();
            _acceleration = acceleration.Rand();
            _gravity = gravity;
            _maxDistance = maxDistance;
            _roll = roll.Rand();

            Position = _startPoint;
            Rotation = _startRotation;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="startRotation">向き</param>
        /// <param name="context">初期化パラメータ</param>
        public ShotBulletProjectile(Vector3 startPoint, Quaternion startRotation, Context context)
            : this(startPoint, startRotation, context.startSpeed, context.acceleration, context.gravity, context.maxDistance, context.roll) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            _rollRotation = Quaternion.Euler(0.0f, 0.0f, _roll);
            Position = _startPoint;
            Rotation = _startRotation * _rollRotation;
            _velocity = _startRotation * Vector3.forward * _startSpeed;
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
            var direction = _velocity.normalized;
            var acceleration = direction * _acceleration + Vector3.down * _gravity;
            _velocity += acceleration * deltaTime;

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
                Rotation = Quaternion.LookRotation(deltaPos) * _rollRotation;
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