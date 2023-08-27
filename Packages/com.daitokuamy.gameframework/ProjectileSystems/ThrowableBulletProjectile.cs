using System;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 投擲用Projectile
    /// </summary>
    public class ThrowableBulletProjectile : IBulletProjectile {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Context {
            [Tooltip("開始座標")]
            public Vector3 startPoint;
            [Tooltip("初速度")]
            public Vector3 startVelocity;
            [Tooltip("重力加速度")]
            public float gravity;
            [Tooltip("反射するオブジェクトのレイヤーマスク")]
            public LayerMask reflectLayerMask;
            [Tooltip("反発係数")]
            public float cor;
            [Tooltip("半径")]
            public float radius;
            [Tooltip("終了時間")]
            public float duration;
            [Tooltip("オブジェクトの傾き")]
            public float tilt;
        }

        private readonly Vector3 _startPoint;
        private readonly Vector3 _startVelocity;
        private readonly float _gravity;
        private readonly int _reflectLayerMask;
        private readonly float _cor;
        private readonly float _radius;
        private readonly float _duration;
        private readonly float _tilt;

        private bool _stopped;
        private Vector3 _velocity;
        private float _timer;
        private Quaternion _tiltRotation;
        private RaycastHit[] _raycastHits;

        // 座標
        public Vector3 Position { get; private set; }

        // 姿勢
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startPoint">始点</param>
        /// <param name="startVelocity">初速度</param>
        /// <param name="gravity">重力加速度</param>
        /// <param name="reflectLayerMask">反射対象のレイヤーマスク</param>
        /// <param name="cor">反発係数</param>
        /// <param name="radius">半径</param>
        /// <param name="duration">終了時間</param>
        /// <param name="tilt">オブジェクトの傾き</param>
        public ThrowableBulletProjectile(Vector3 startPoint, Vector3 startVelocity, float gravity, int reflectLayerMask, float cor, float radius, float duration, float tilt) {
            _startPoint = startPoint;
            _startVelocity = startVelocity;
            _gravity = gravity;
            _reflectLayerMask = reflectLayerMask;
            _cor = Mathf.Max(0, cor);
            _radius = radius;
            _duration = duration;
            _tilt = tilt;

            _raycastHits = new RaycastHit[4];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">初期化パラメータ</param>
        public ThrowableBulletProjectile(Context context)
            : this(context.startPoint, context.startVelocity, context.gravity, context.reflectLayerMask, context.cor, context.radius, context.duration, context.tilt) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectile.Start() {
            _tiltRotation = Quaternion.Euler(0.0f, 0.0f, _tilt);
            Position = _startPoint;
            Rotation = Quaternion.LookRotation(_startVelocity) * _tiltRotation;
            _velocity = _startVelocity;
            _timer = _duration;
            _stopped = false;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool IProjectile.Update(float deltaTime) {
            // 速度更新
            _velocity += Vector3.down * (_gravity * deltaTime);

            // 移動量
            var deltaPos = _velocity * deltaTime;
            
            // 座標からレイを飛ばして反射結果の位置まで更新する
            void Reflect(Vector3 position, Vector3 vector) {
                var nextPos = position + vector;
                var hitCount = 0;
                if (_radius > float.Epsilon) {
                    hitCount = Physics.SphereCastNonAlloc(position, _radius, vector.normalized, _raycastHits, deltaPos.magnitude, _reflectLayerMask);
                }
                else {
                    hitCount = Physics.RaycastNonAlloc(position, vector.normalized, _raycastHits, deltaPos.magnitude, _reflectLayerMask);
                }
                
                if (hitCount <= 0) {
                    // 最終位置の決定
                    deltaPos = nextPos - Position;
                    return;
                }

                var resultIndex = 0;
                var hitResult = _raycastHits[resultIndex];
                while (hitResult.distance <= float.Epsilon) {
                    if (resultIndex >= hitCount - 1) {
                        // 最終位置の決定
                        deltaPos = nextPos - Position;
                        return;
                    }

                    resultIndex++;
                    hitResult = _raycastHits[resultIndex];
                }
                
                // 衝突後のベクトルを求める
                var hitCenter = hitResult.point + hitResult.normal * _radius;
                var reflectVec = Vector3.Reflect(nextPos - hitCenter, hitResult.normal) * _cor;
                
                // 速度を更新
                _velocity = reflectVec.normalized * (_velocity.magnitude * _cor);
                
                // 複数反射している可能性があるため再帰処理
                Reflect(hitCenter, reflectVec);
            }

            // 地面判定
            Reflect(Position, deltaPos);

            // 座標更新
            Position += deltaPos;

            // 向き更新
            if (deltaPos.sqrMagnitude > 0.000001f) {
                Rotation = Quaternion.LookRotation(deltaPos) * _tiltRotation;
            }
            
            // 時間更新
            _timer -= deltaTime;

            return !_stopped && _timer > 0.0f;
        }

        /// <summary>
        /// 飛翔終了
        /// </summary>
        void IProjectile.Stop() {
            _stopped = true;
        }
    }
}