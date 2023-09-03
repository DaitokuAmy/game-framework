using GameFramework.ProjectileSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用の粒子ビーム攻撃イベント
    /// </summary>
    public class BattleParticleBeamProjectileRangeEvent : BattleBeamProjectileRangeEvent {
        [Tooltip("ビーム内容")]
        public ParticleBeamProjectile.Context context;
    }

    /// <summary>
    /// BattleParticleBeamProjectileRangeEventのハンドラ基底
    /// </summary>
    public class BattleParticleBeamProjectileRangeEventHandler : BattleBeamProjectileRangeEventHandler<BattleParticleBeamProjectileRangeEvent> {
        /// <summary>
        /// Projectileの取得
        /// </summary>
        protected override IBeamProjectile GetProjectile(Transform baseTransform, BattleParticleBeamProjectileRangeEvent sequenceEvent) {
            return new ParticleBeamProjectile(baseTransform, sequenceEvent.relativePosition, Quaternion.Euler(sequenceEvent.relativeAngles), sequenceEvent.context);
        }
    }
}