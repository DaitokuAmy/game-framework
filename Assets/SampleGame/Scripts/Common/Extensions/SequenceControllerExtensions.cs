using System;
using ActionSequencer;
using GameFramework.BodySystems;
using GameFramework.CollisionSystems;
using GameFramework.ProjectileSystems;
using SampleGame.Battle;
using SampleGame.SequenceEvents;

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
            
            self.BindRangeEventHandler<BattleParticleBeamProjectileRangeEvent, BattleParticleBeamProjectileRangeEventHandler>(handler => {
                handler.Setup(collisionManager, projectileObjectManager, collisionListener, raycastCollisionListener, actor, targetLayerMask, checkHitFunc);
            });
        }
        
        /// <summary>
        /// Body用ギミックイベントのバインド
        /// </summary>
        public static void BindBodyGimmickEvent(this IReadOnlySequenceController self, Body body) {
            var gimmickController = body.GetController<GimmickController>();
            if (gimmickController == null) {
                return;
            }
            
            self.BindSignalEventHandler<BodyActiveGimmickSingleEvent, BodyActiveGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyAnimationGimmickSingleEvent, BodyAnimationGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyInvokeGimmickSingleEvent, BodyInvokeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyFloatChangeGimmickSingleEvent, BodyFloatChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyVectorChangeGimmickSingleEvent, BodyVectorChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyColorChangeGimmickSingleEvent, BodyColorChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            self.BindSignalEventHandler<BodyHdrColorChangeGimmickSingleEvent, BodyHdrColorChangeGimmickSingleEventHandler>(handler => handler.Setup(gimmickController));
            
            self.BindRangeEventHandler<BodyActiveGimmickRangeEvent, BodyActiveGimmickRangeEventHandler>(handler => handler.Setup(gimmickController));
            self.BindRangeEventHandler<BodyAnimationGimmickRangeEvent, BodyAnimationGimmickRangeEventHandler>(handler => handler.Setup(gimmickController));
        }
    }
}