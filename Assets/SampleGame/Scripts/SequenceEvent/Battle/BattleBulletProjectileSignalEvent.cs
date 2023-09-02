using System;
using ActionSequencer;
using GameFramework.CollisionSystems;
using GameFramework.Core;
using GameFramework.ProjectileSystems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃イベント基底
    /// </summary>
    public abstract class BattleBulletProjectileSignalEvent : SignalSequenceEvent {
        [Header("Projectile")]
        [Tooltip("ヒット最大数")]
        public int hitCount = 1;
        [Tooltip("発射するProjectile用Prefab")]
        public GameObject prefab;
        [Tooltip("スケール")]
        public float scale = 1.0f;
        [Tooltip("発射とするロケーター名")]
        public string locatorName = "";
        [Tooltip("発射位置のオフセット座標")]
        public MinMaxVector3 offsetPosition;
        [Tooltip("発射位置のオフセット向き")]
        public MinMaxVector3 offsetAngles;
        [Tooltip("ターゲットがいなかった場合の着弾距離")]
        public float emptyRange = 50.0f;
        [Tooltip("レイキャストでダメージを与えるか")]
        public bool raycastDamage = true;
        [Tooltip("着弾時のコリジョン半径")]
        public float exitCollisionRadius = 0.0f;
        [Tooltip("着弾時のコリジョン発生時間")]
        public float exitCollisionDuration = 0.0f;

        [Header("Target")]
        [Tooltip("ターゲットのロケーター名")]
        public string targetLocatorName = "";
        [Tooltip("ターゲット位置へのオフセット")]
        public MinMaxVector3 targetPositionOffset;
    }

    /// <summary>
    /// BattleProjectileSignalEventのハンドラ基底
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BattleProjectileSignalEventHandler<T> : SignalSequenceEventHandler<T>
        where T : BattleBulletProjectileSignalEvent {
        private CollisionManager _collisionManager;
        private ProjectileObjectManager _projectileObjectManager;
        private ICollisionListener _collisionListener;
        private IRaycastCollisionListener _raycastListener;
        private BattleCharacterActor _actor;
        private int _layerMask;
        private Func<RaycastHitResult, bool> _checkHitFunc;

        private Transform _targetTransform;
        private Vector3 _targetPosition;
        private Vector3 _targetOffset;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(CollisionManager collisionManager, ProjectileObjectManager projectileObjectManager, ICollisionListener collisionListener, IRaycastCollisionListener raycastListener, BattleCharacterActor characterActor, int layerMask, Func<RaycastHitResult, bool> checkHitFunc) {
            _collisionManager = collisionManager;
            _projectileObjectManager = projectileObjectManager;
            _collisionListener = collisionListener;
            _raycastListener = raycastListener;
            _actor = characterActor;
            _layerMask = layerMask;
            _checkHitFunc = checkHitFunc;
        }

        /// <summary>
        /// 発動処理
        /// </summary>
        protected override void OnInvoke(T sequenceEvent) {
            var baseTrans = _actor.Body.Locators[sequenceEvent.locatorName];
            
            // ターゲット位置のオフセット
            _targetOffset = sequenceEvent.targetPositionOffset.Rand();
            _targetOffset = _actor.Body.Transform.TransformDirection(_targetOffset);

            // ターゲット情報の記憶
            var targetActor = _actor.TargetCharacterActor;
            if (targetActor != null) {
                _targetPosition = Vector3.zero;
                _targetTransform = targetActor.Body.Locators[sequenceEvent.targetLocatorName];
            }
            else {
                _targetTransform = null;
                _targetPosition = _actor.Body.Transform.TransformPoint(0.0f, 0.0f, sequenceEvent.emptyRange);
            }
            
            var projectile = GetProjectile(baseTrans, sequenceEvent);

            // Projectileの再生
            _projectileObjectManager.Play(
                _raycastListener, sequenceEvent.prefab, projectile, Vector3.one * sequenceEvent.scale, _layerMask,
                sequenceEvent.hitCount, null,
                _actor.Body.LayeredTime, _checkHitFunc,
                (pos, rot) => OnUpdatedTransform(projectile, pos, rot), onExit: () => {
                    // 着弾時にコリジョンを発生させる必要があれば発生
                    if (sequenceEvent.exitCollisionRadius > float.Epsilon && sequenceEvent.exitCollisionDuration >= 0.0f) {
                        _collisionManager.Register(_collisionListener, new SphereCollision(projectile.Position, sequenceEvent.exitCollisionRadius), _layerMask, null, sequenceEvent.exitCollisionDuration);
                    }
                });
        }

        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected abstract IBulletProjectile GetProjectile(Transform baseTransform, T sequenceEvent);

        /// <summary>
        /// ProjectileのTransform更新時通知
        /// </summary>
        protected virtual void OnUpdatedTransform(IBulletProjectile projectile, Vector3 position, Quaternion rotation) {
        }

        /// <summary>
        /// ターゲット座標の取得
        /// </summary>
        protected Vector3 GetTargetPoint() {
            if (_targetTransform == null) {
                return _targetPosition + _targetOffset;
            }

            return _targetTransform.position + _targetOffset;
        }
    }
}