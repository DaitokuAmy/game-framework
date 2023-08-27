using GameFramework.Core;
using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃
    /// </summary>
    public class BattleCustomBulletProjectileSignalEvent : BattleProjectileSignalEvent {
        [Tooltip("飛翔体パラメータ")]
        public CustomBulletProjectile.Context projectileContext;
    }

    /// <summary>
    /// BattleCustomBulletProjectileSignalEvent用のハンドラ
    /// </summary>
    public class BattleCustomBulletProjectileSignalEventHandler : BattleProjectileSignalEventHandler<BattleCustomBulletProjectileSignalEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBulletProjectile GetProjectile(Transform baseTransform, BattleCustomBulletProjectileSignalEvent sequenceEvent) {
            var context = sequenceEvent.projectileContext;
            var startPoint = sequenceEvent.offsetPosition.Rand();
            var endPoint = GetTargetPoint();
            if (baseTransform != null) {
                startPoint = baseTransform.TransformPoint(startPoint);
            }

            return new CustomBulletProjectile(startPoint, endPoint, context);
        }

        /// <summary>
        /// Transform更新時の処理
        /// </summary>
        protected override void OnUpdatedTransform(IProjectile projectile, Vector3 position, Quaternion rotation) {
            // 追従先を更新
            if (projectile is CurveBulletProjectile curveProjectile) {
                curveProjectile.EndPoint = GetTargetPoint();
            }
        }
    }
}