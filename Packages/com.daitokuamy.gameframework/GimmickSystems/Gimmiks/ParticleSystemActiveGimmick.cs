using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// ParticleSystemのアクティブコントロールをするGimmick
    /// </summary>
    public class ParticleSystemActiveGimmick : ActiveGimmick {
        [SerializeField, Tooltip("Active制御する対象")]
        private ParticleSystem[] _targets;

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        protected override void ActivateInternal() {
            foreach (var ps in _targets) {
                if (ps == null) {
                    continue;
                }

                ps.Play(true);
            }
        }

        /// <summary>
        /// 非アクティブ化処理
        /// </summary>
        protected override void DeactivateInternal() {
            foreach (var ps in _targets) {
                if (ps == null) {
                    continue;
                }

                ps.Stop(true);
            }
        }
    }
}