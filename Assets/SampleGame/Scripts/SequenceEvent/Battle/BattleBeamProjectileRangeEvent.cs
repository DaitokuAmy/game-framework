using System;
using ActionSequencer;
using GameFramework.CollisionSystems;
using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用のビーム攻撃イベント基底
    /// </summary>
    public abstract class BattleBeamProjectileRangeEvent : RangeSequenceEvent {
        [Header("Projectile")]
        [Tooltip("ヒット最大数")]
        public int hitCount = -1;
        [Tooltip("発射するProjectile用Prefab")]
        public GameObject prefab;
        [Tooltip("スケール")]
        public float scale = 1.0f;
        [Tooltip("発生基点とするロケーター名")]
        public string locatorName = "";
        [Tooltip("発生基点の座標オフセット")]
        public Vector3 relativePosition;
        [Tooltip("発生基点の向きオフセット")]
        public Vector3 relativeAngles;
        [Tooltip("レイキャストでダメージを与えるか")]
        public bool raycastDamage = true;
        [Tooltip("着弾時のコリジョン半径")]
        public float exitCollisionRadius = 0.0f;
        [Tooltip("着弾時のコリジョン発生時間")]
        public float exitCollisionDuration = 0.0f;
    }

    /// <summary>
    /// BattleBeamProjectileRangeEventのハンドラ基底
    /// </summary>
    public abstract class BattleBeamProjectileRangeEventHandler<TEvent> : RangeSequenceEventHandler<TEvent>
        where TEvent : BattleBeamProjectileRangeEvent {
        private CollisionManager _collisionManager;
        private ProjectileObjectManager _projectileObjectManager;
        private ICollisionListener _collisionListener;
        private IRaycastCollisionListener _raycastListener;
        private BattleCharacterActor _actor;
        private int _layerMask;
        private Func<RaycastHitResult, bool> _checkHitFunc;

        private ProjectileObjectManager.Handle _handle;

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
        /// 開始処理
        /// </summary>
        protected override void OnEnter(TEvent sequenceEvent) {
            var baseTrans = _actor.Body.Locators[sequenceEvent.locatorName];
            var projectile = GetProjectile(baseTrans, sequenceEvent);

            _handle = _projectileObjectManager.Play(
                _raycastListener, sequenceEvent.prefab, projectile, Vector3.one * sequenceEvent.scale, _layerMask,
                sequenceEvent.hitCount, null, _actor.Body.LayeredTime, -1,
                _checkHitFunc, onExit: () => {
                    // 着弾時にコリジョンを発生させる必要があれば発生
                    if (sequenceEvent.exitCollisionRadius > float.Epsilon && sequenceEvent.exitCollisionDuration >= 0.0f) {
                        _collisionManager.Register(_collisionListener, new SphereCollision(projectile.HeadPosition, sequenceEvent.exitCollisionRadius), _layerMask, null,
                            sequenceEvent.exitCollisionDuration);
                    }
                });
        }

        /// <summary>
        /// 停止処理
        /// </summary>
        protected override void OnExit(TEvent sequenceEvent) {
            _handle.Dispose();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void OnCancel(TEvent sequenceEvent) {
            OnExit(sequenceEvent);
        }

        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected abstract IBeamProjectile GetProjectile(Transform baseTransform, TEvent sequenceEvent);
    }
}