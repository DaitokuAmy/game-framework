using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の遠距離攻撃
    /// </summary>
    public class BattleCurveProjectileSignalEvent : BattleProjectileSignalEvent {
        [Tooltip("飛翔体パラメータ")]
        public CurveBulletProjectile.Context projectileContext;
    }

    /// <summary>
    /// BattleCurveProjectileSignalEvent用のハンドラ
    /// </summary>
    public class BattleCurveProjectileSignalEventHandler : BattleProjectileSignalEventHandler<BattleCurveProjectileSignalEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBulletProjectile GetProjectile(Transform baseTransform, BattleCurveProjectileSignalEvent sequenceEvent) {
            var context = sequenceEvent.projectileContext;
            if (baseTransform != null) {
                context.startPoint = baseTransform.TransformPoint(context.startPoint);
                context.endPoint = GetTargetPoint();
            }

            return new CurveBulletProjectile(context);
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