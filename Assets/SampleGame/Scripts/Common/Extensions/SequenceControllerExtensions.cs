using System;
using ActionSequencer;
using GameFramework.CollisionSystems;
using GameFramework.ProjectileSystems;
using SampleGame.Battle;

namespace SampleGame {
    /// <summary>
    /// SequenceController用の拡張メソッド定義クラス
    /// </summary>
    public static partial class SequenceControllerExtensions {
        /// <summary>
        /// バトル用遠距離攻撃イベントのバインド
        /// </summary>
        public static void BindBattleProjectileEvent(this IReadOnlySequenceController self,
            CollisionManager collisionManager, ProjectileObjectManager projectileObjectManager,
            ICollisionListener collisionListener, IRaycastCollisionListener raycastCollisionListener,
            BattleCharacterActor actor,
            int targetLayerMask, Func<RaycastHitResult, bool> checkHitFunc) {
            self.BindSignalEventHandler<BattleShotBulletProjectileSignalEvent, BattleShotBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(collisionManager, projectileObjectManager, collisionListener, raycastCollisionListener, actor, targetLayerMask, checkHitFunc);
            });
            self.BindSignalEventHandler<BattleHomingBulletProjectileSignalEvent, BattleHomingBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(collisionManager, projectileObjectManager, collisionListener, raycastCollisionListener, actor, targetLayerMask, checkHitFunc);
            });
            self.BindSignalEventHandler<BattleCustomBulletProjectileSignalEvent, BattleCustomBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(collisionManager, projectileObjectManager, collisionListener, raycastCollisionListener, actor, targetLayerMask, checkHitFunc);
            });
            self.BindSignalEventHandler<BattleThrowableBulletProjectileSignalEvent, BattleThrowableBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(collisionManager, projectileObjectManager, collisionListener, raycastCollisionListener, actor, targetLayerMask, checkHitFunc);
            });
            self.BindSignalEventHandler<BattleSplineBulletProjectileSignalEvent, BattleSplineBulletProjectileSignalEventHandler>(handler => {
                handler.Setup(collisionManager, projectileObjectManager, collisionListener, raycastCollisionListener, actor, targetLayerMask, checkHitFunc);
            });
            
            self.BindRangeEventHandler<BattleBeamProjectileRangeEvent, BattleBeamProjectileRangeEventHandler>(handler => {
                handler.Setup(collisionManager, projectileObjectManager, collisionListener, raycastCollisionListener, actor, targetLayerMask, checkHitFunc);
            });
        }
    }
}