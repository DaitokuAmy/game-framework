using System;
using ActionSequencer;
using GameFramework.CollisionSystems;
using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用のビーム攻撃イベント基底
    /// </summary>
    public class BattleBeamProjectileRangeEvent : RangeSequenceEvent {
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
        [Tooltip("ビーム内容")]
        public BeamProjectile.Context context;
    }

    /// <summary>
    /// BattleProjectileRangeEventのハンドラ基底
    /// </summary>
    public class BattleBeamProjectileRangeEventHandler : RangeSequenceEventHandler<BattleBeamProjectileRangeEvent> {
        private ProjectileObjectManager _projectileObjectManager;
        private IRaycastCollisionListener _listener;
        private BattleCharacterActor _actor;
        private int _layerMask;
        private Func<RaycastHitResult, bool> _checkHitFunc;

        private ProjectileObjectManager.Handle _handle;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(ProjectileObjectManager projectileObjectManager, IRaycastCollisionListener listener, BattleCharacterActor characterActor, int layerMask, Func<RaycastHitResult, bool> checkHitFunc) {
            _projectileObjectManager = projectileObjectManager;
            _listener = listener;
            _actor = characterActor;
            _layerMask = layerMask;
            _checkHitFunc = checkHitFunc;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        protected override void OnEnter(BattleBeamProjectileRangeEvent sequenceEvent) {
            var baseTrans = _actor.Body.Locators[sequenceEvent.locatorName];
            var projectile = new BeamProjectile(baseTrans, sequenceEvent.relativePosition, Quaternion.Euler(sequenceEvent.relativeAngles), sequenceEvent.context);

            _handle = _projectileObjectManager.Play(
                _listener, sequenceEvent.prefab, projectile, Vector3.one * sequenceEvent.scale, _layerMask,
                sequenceEvent.hitCount, _actor.Body.LayeredTime,
                null, _checkHitFunc);
        }

        /// <summary>
        /// 停止処理
        /// </summary>
        protected override void OnExit(BattleBeamProjectileRangeEvent sequenceEvent) {
            _handle.Dispose();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        protected override void OnCancel(BattleBeamProjectileRangeEvent sequenceEvent) {
            OnExit(sequenceEvent);
        }
    }
}