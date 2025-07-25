using System;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 粒子ビーム用Projectile
    /// </summary>
    public class ParticleBeamProjectileController : IBeamProjectileController {
        /// <summary>
        /// 初期化用データ 
        /// </summary>
        [Serializable]
        public struct Settings {
            [Tooltip("先端速度")]
            public float startSpeed;
            [Tooltip("末端速度")]
            public float endSpeed;
            [Tooltip("障害となるレイヤーマスク")]
            public LayerMask obstacleLayerMask;
            [Tooltip("半径")]
            public float radius;
            [Tooltip("最大距離")]
            public float maxDistance;
            [Tooltip("オブジェクトの傾き")]
            public float tilt;
        }

        private readonly Transform _baseTransform;
        private readonly Vector3 _baseOffsetPosition;
        private readonly Quaternion _baseOffsetRotation;
            
        private readonly float _headSpeed;
        private readonly float _tailSpeed;
        private readonly int _obstacleLayerMask;
        private readonly float _radius;
        private readonly float _maxDistance;
        private readonly float _tilt;

        private bool _stopped;
        private Matrix4x4 _prevBaseMatrix;
        private Quaternion _prevBaseRotation;
        private float _headDistance;
        private float _tailDistance;
        private Quaternion _tiltRotation;
        private RaycastHit[] _raycastHits;

        /// <summary>先端座標</summary>
        public Vector3 HeadPosition { get; private set; }
        /// <summary>末端座標</summary>
        public Vector3 TailPosition { get; private set; }
        /// <summary>姿勢</summary>
        public Quaternion Rotation { get; private set; }
        /// <summary>飛翔体の長さ</summary>
        public float Distance { get; private set; }
        /// <summary>太さ</summary>
        public float Thickness { get; private set; }
        /// <summary>実体化しているか</summary>
        public bool IsSolid => true;
        /// <summary>衝突中か</summary>
        public bool IsHitting { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="baseTransform">基点となるTransform</param>
        /// <param name="baseOffsetPosition">基点となるTransformのオフセット座標</param>
        /// <param name="baseOffsetRotation">基点となるTransformのオフセット向き</param>
        /// <param name="headSpeed">先端速度</param>
        /// <param name="tailSpeed">末端速度</param>
        /// <param name="obstacleLayerMask">障害用レイヤーマスク</param>
        /// <param name="radius">半径</param>
        /// <param name="maxDistance">最大距離</param>
        /// <param name="tilt">傾き</param>
        public ParticleBeamProjectileController(Transform baseTransform, Vector3 baseOffsetPosition, Quaternion baseOffsetRotation, float headSpeed, float tailSpeed, int obstacleLayerMask, float radius, float maxDistance, float tilt) {
            _baseTransform = baseTransform;
            _baseOffsetPosition = baseOffsetPosition;
            _baseOffsetRotation = baseOffsetRotation;
            _headSpeed = headSpeed;
            _tailSpeed = tailSpeed;
            _obstacleLayerMask = obstacleLayerMask;
            _radius = radius;
            _maxDistance = maxDistance;
            _tilt = tilt;
            
            _raycastHits = new RaycastHit[1];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="baseTransform">基点となるTransform</param>
        /// <param name="baseOffsetPosition">基点となるTransformのオフセット座標</param>
        /// <param name="baseOffsetRotation">基点となるTransformのオフセット向き</param>
        /// <param name="settings">初期化パラメータ</param>
        public ParticleBeamProjectileController(Transform baseTransform, Vector3 baseOffsetPosition, Quaternion baseOffsetRotation, Settings settings)
            : this(baseTransform, baseOffsetPosition, baseOffsetRotation,
                settings.startSpeed,
                settings.endSpeed, 
                settings.obstacleLayerMask, 
                settings.radius, 
                settings.maxDistance, 
                settings.tilt) {
        }

        /// <summary>
        /// 飛翔開始
        /// </summary>
        void IProjectileController.Start() {
            _stopped = false;
            _headDistance = 0.0f;
            _tailDistance = 0.0f;
            _tiltRotation = Quaternion.Euler(0.0f, 0.0f, _tilt);
            
            HeadPosition = CalcBeamPoint(0.0f);
            TailPosition = HeadPosition;
            Rotation = CalcBeamRotation();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool IProjectileController.Update(float deltaTime) {
            // 照射距離更新
            if (_stopped) {
                _tailDistance += _tailSpeed * deltaTime;
            }
            else {
                _headDistance += _headSpeed * deltaTime;
            }

            var point = CalcBeamPoint(_headDistance);
            var origin = CalcBeamPoint(_tailDistance);
            var vector = point - origin;
            var end = _headDistance <= _tailDistance + float.Epsilon;
            
            // 当たり判定チェック
            var hitCount = 0;
            if (!end && vector.sqrMagnitude >= float.Epsilon) {
                if (_radius > float.Epsilon) {
                    hitCount = Physics.SphereCastNonAlloc(origin, _radius, vector.normalized, _raycastHits, vector.magnitude, _obstacleLayerMask);
                }
                else {
                    hitCount = Physics.RaycastNonAlloc(origin, vector.normalized, _raycastHits, vector.magnitude, _obstacleLayerMask);
                }
            }

            if (hitCount > 0) {
                // 距離の調整
                _headDistance = _raycastHits[0].distance + _tailDistance;
            }

            // 衝突状態の更新
            IsHitting = hitCount > 0 || end;
            
            // 最大距離の調整
            _headDistance = Mathf.Min(_headDistance, _maxDistance);
            
            // 末端が先端をこえないように
            _tailDistance = Mathf.Min(_tailDistance, _headDistance);
            
            // 座標更新
            HeadPosition = CalcBeamPoint(_headDistance);
            TailPosition = CalcBeamPoint(_tailDistance);
            
            // 長さ更新
            Distance = _headDistance - _tailDistance;
            
            // 姿勢更新
            Rotation = CalcBeamRotation();
            
            // 停止してなければTransformの情報を記憶
            if (!_stopped) {
                _prevBaseMatrix = _baseTransform != null ? _baseTransform.localToWorldMatrix : Matrix4x4.identity;
                _prevBaseRotation = _baseTransform != null ? _baseTransform.rotation : Quaternion.identity;
            }

            // 完了チェック
            if (_stopped && _headDistance <= _tailDistance + float.Epsilon) {
                IsHitting = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 飛翔終了
        /// </summary>
        void IProjectileController.Stop(Vector3? stopPosition) {
            _stopped = true;
        }

        /// <summary>
        /// ビームポイントの計算
        /// </summary>
        private Vector3 CalcBeamPoint(float distance) {
            // 停止中ならキャッシュしたMatrixを使用
            if (_stopped) {
                return _prevBaseMatrix.MultiplyPoint(_baseOffsetPosition + distance * (_baseOffsetRotation * Vector3.forward));
            }
            
            if (_baseTransform == null) {
                return _baseOffsetPosition + distance * (_baseOffsetRotation * Vector3.forward);
            }

            return _baseTransform.TransformPoint(_baseOffsetPosition + distance * (_baseOffsetRotation * Vector3.forward));
        }

        /// <summary>
        /// ビーム向きの計算
        /// </summary>
        private Quaternion CalcBeamRotation() {
            // 停止中ならキャッシュしたQuaternionを使用
            if (_stopped) {
                return _prevBaseRotation * _baseOffsetRotation * _tiltRotation;
            }

            if (_baseTransform == null) {
                return _baseOffsetRotation * _tiltRotation;
            }
            
            return _baseTransform.rotation * _baseOffsetRotation * _tiltRotation;
        }
    }
}