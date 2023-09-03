using GameFramework.Core;
using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃
    /// </summary>
    public class BattleShotBulletProjectileSignalEvent : BattleBulletProjectileSignalEvent {
        [Tooltip("飛翔体パラメータ")]
        public ShotBulletProjectile.Context projectileContext;
    }

    /// <summary>
    /// BattleShotBulletProjectileSignalEvent用のハンドラ
    /// </summary>
    public class BattleShotBulletProjectileSignalEventHandler : BattleProjectileSignalEventHandler<BattleShotBulletProjectileSignalEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBulletProjectile GetProjectile(Transform baseTransform, BattleShotBulletProjectileSignalEvent sequenceEvent) {
            var context = sequenceEvent.projectileContext;
            var startPoint = sequenceEvent.offsetPosition.Rand();
            var startRotation = Quaternion.Euler(sequenceEvent.offsetAngles.Rand());
            if (baseTransform != null) {
                startPoint = baseTransform.TransformPoint(startPoint);
                startRotation = baseTransform.rotation * startRotation;
            }

            return new ShotBulletProjectile(startPoint, startRotation, context);
        }
    }
}