using GameFramework.Core;
using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃
    /// </summary>
    public class BattleHomingBulletProjectileSignalEvent : BattleProjectileSignalEvent {
        [Space]
        [Header("Projectile Parameter")]
        [Tooltip("飛翔体パラメータ")]
        public HomingBulletProjectile.Context projectileContext;
    }

    /// <summary>
    /// BattleHomingBulletProjectileSignalEvent用のハンドラ
    /// </summary>
    public class BattleHomingBulletProjectileSignalEventHandler : BattleProjectileSignalEventHandler<BattleHomingBulletProjectileSignalEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBulletProjectile GetProjectile(Transform baseTransform, BattleHomingBulletProjectileSignalEvent sequenceEvent) {
            var context = sequenceEvent.projectileContext;
            var startPoint = sequenceEvent.offsetPosition.Rand();
            var startRotation = Quaternion.Euler(sequenceEvent.offsetAngles.Rand());
            var endPoint = GetTargetPoint();
            if (baseTransform != null) {
                startPoint = baseTransform.TransformPoint(startPoint);
                startRotation = baseTransform.rotation * startRotation;
            }

            return new HomingBulletProjectile(startPoint, startRotation, endPoint, context);
        }

        /// <summary>
        /// Transform更新時の処理
        /// </summary>
        protected override void OnUpdatedTransform(IProjectile projectile, Vector3 position, Quaternion rotation) {
            // 追従先を更新
            if (projectile is HomingBulletProjectile homingProjectile) {
                homingProjectile.EndPoint = GetTargetPoint();
            }
        }
    }
}