using GameFramework.Core;
using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃(投擲物)
    /// </summary>
    public class BattleAimedThrowableBulletProjectileSignalEvent : BattleBulletProjectileSignalEvent {
        [Tooltip("飛翔体パラメータ")]
        public AimedThrowableBulletProjectile.Context projectileContext;
    }

    /// <summary>
    /// BattleAimedThrowableProjectileSignalEvent用のハンドラ
    /// </summary>
    public class BattleAimedThrowableBulletProjectileSignalEventHandler : BattleProjectileSignalEventHandler<BattleAimedThrowableBulletProjectileSignalEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBulletProjectile GetProjectile(Transform baseTransform, BattleAimedThrowableBulletProjectileSignalEvent sequenceEvent) {
            var context = sequenceEvent.projectileContext;
            var startPoint = sequenceEvent.offsetPosition.Rand();
            var startRotation = Quaternion.Euler(sequenceEvent.offsetAngles.Rand());
            if (baseTransform != null) {
                startPoint = baseTransform.TransformPoint(startPoint);
                startRotation = baseTransform.rotation * startRotation;
            }
            var endPoint = GetTargetPoint();

            return new AimedThrowableBulletProjectile(startPoint, startRotation, endPoint, context);
        }
    }
}