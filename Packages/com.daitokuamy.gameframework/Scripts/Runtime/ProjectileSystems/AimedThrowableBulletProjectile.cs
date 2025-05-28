using System;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ポイント投擲用Projectile
    /// </summary>
    public class AimedThrowableBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("重力加速度")]
            public float gravity;
            [Tooltip("オブジェクトの傾き")]
            public float roll;
        }

        private readonly Vector3 _startPoint;
        private readonly Quaternion _startRotation;
        private readonly Vector3 _endPoint;
        private readonly float _gravity;
        private readonly float _roll;

        private bool _stopped;
        private Vector3 _velocity;
        private Quaternion _rollRotation;
        private float _timer;

        /// <summary>座標</summary>
        public Vector3 Position { get; private set; }
        /// <summary>姿勢</summary>
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="startRotation">向き</param>
        /// <param name="endPoint">終了座標</param>
        /// <param name="gravity">重力加速度</param>
        /// <param name="roll">オブジェクトの傾き</param>
        public AimedThrowableBulletProjectile(Vector3 startPoint, Quaternion startRotation, Vector3 endPoint, float gravity, float roll) {
            _startPoint = startPoint;
            _startRotation = startRotation;
            _endPoint = endPoint;
            _gravity = gravity;
            _roll = roll;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="startRotation">向き</param>
        /// <param name="endPoint">終了座標</param>
        /// <param name="context">初期化パラメータ</param>
        public AimedThrowableBulletProjectile(Vector3 startPoint, Quaternion startRotation, Vector3 endPoint, Context context)
            : this(startPoint, startRotation, endPoint, context.gravity, context.roll) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            _rollRotation = Quaternion.Euler(0.0f, 0.0f, _roll);
            Position = _startPoint;
            Rotation = _startRotation * _rollRotation;

            // 投射パラメータの計算
            if (CalcThrowParameter(_startPoint, _startRotation, _endPoint, _gravity, out var speed, out var duration)) {
                _velocity = _startRotation * Vector3.forward * speed;
                _timer = duration;
                _stopped = false;
            }
            else {
                _stopped = true;
            }
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
            _velocity += Vector3.down * (_gravity * deltaTime);

            // 移動量
            var deltaPos = _velocity * deltaTime;

            // 着弾予定位置より下にいったら押し戻す
            if (Position.y + deltaPos.y < _endPoint.y) {
                var scale = (_endPoint.y - Position.y) / deltaPos.y;
                deltaPos *= scale;
            }

            // 座標更新
            Position += deltaPos;

            // 向き更新
            if (deltaPos.sqrMagnitude > 0.000001f) {
                Rotation = Quaternion.LookRotation(deltaPos) * _rollRotation;
            }

            // 時間更新
            _timer -= deltaTime;

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
        /// 投射するパラメータを計算
        /// </summary>
        private bool CalcThrowParameter(Vector3 startPos, Quaternion startRot, Vector3 endPos, float gravity, out float speed, out float duration) {
            speed = 0.0f;
            duration = 0.0f;

            // 目的地への奥行を計算
            var direction = startRot * Vector3.forward;
            var directionXZ = direction;
            directionXZ.y = 0.0f;
            directionXZ.Normalize();
            var vector = endPos - startPos;
            vector.y = 0.0f;
            var depth = Vector3.Dot(directionXZ, vector);

            // 裏側であれば失敗
            if (depth < 0.0f) {
                return false;
            }

            // 速度と時間を求める
            var height = startPos.y - endPos.y;
            var sinTheta = direction.y;
            var cosTheta = Mathf.Sqrt(1.0f - sinTheta * sinTheta);
            speed = Mathf.Sqrt(gravity * depth * depth / (depth * 2 * sinTheta * cosTheta + 2 * height * cosTheta * cosTheta));
            duration = depth / (cosTheta * speed);
            return true;
        }
    }
}