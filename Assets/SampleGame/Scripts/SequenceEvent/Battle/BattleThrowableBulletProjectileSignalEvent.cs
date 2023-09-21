using GameFramework.Core;
using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃(投擲物)
    /// </summary>
    public class BattleThrowableBulletProjectileSignalEvent : BattleBulletProjectileSignalEvent {
        [Tooltip("飛翔体パラメータ")]
        public ThrowableBulletProjectile.Context projectileContext;
    }

    /// <summary>
    /// BattleThrowableProjectileSignalEvent用のハンドラ
    /// </summary>
    public class BattleThrowableBulletProjectileSignalEventHandler : BattleProjectileSignalEventHandler<BattleThrowableBulletProjectileSignalEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBulletProjectile GetProjectile(Transform baseTransform, BattleThrowableBulletProjectileSignalEvent sequenceEvent) {
            var context = sequenceEvent.projectileContext;
            var startPoint = sequenceEvent.offsetPosition.Rand();
            var startRotation = Quaternion.Euler(sequenceEvent.offsetAngles.Rand());
            if (baseTransform != null) {
                startPoint = baseTransform.TransformPoint(startPoint);
                startRotation = baseTransform.rotation * startRotation;
            }

            return new ThrowableBulletProjectile(startPoint, startRotation, context);
        }
    }
}