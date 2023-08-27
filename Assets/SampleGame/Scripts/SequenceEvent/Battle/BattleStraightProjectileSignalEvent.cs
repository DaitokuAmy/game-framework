using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃
    /// </summary>
    public class BattleStraightProjectileSignalEvent : BattleProjectileSignalEvent {
        [Tooltip("飛翔体パラメータ")]
        public StraightBulletProjectile.Context projectileContext;
    }

    /// <summary>
    /// BattleStraightProjectileSignalEvent用のハンドラ
    /// </summary>
    public class BattleStraightProjectileSignalEventHandler : BattleProjectileSignalEventHandler<BattleStraightProjectileSignalEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBulletProjectile GetProjectile(Transform baseTransform, BattleStraightProjectileSignalEvent sequenceEvent) {
            var context = sequenceEvent.projectileContext;
            if (baseTransform != null) {
                context.startPoint = baseTransform.TransformPoint(context.startPoint);
                context.startVelocity = baseTransform.TransformVector(context.startVelocity);
                context.acceleration = baseTransform.TransformVector(context.acceleration);
            }

            return new StraightBulletProjectile(context);
        }
    }
}