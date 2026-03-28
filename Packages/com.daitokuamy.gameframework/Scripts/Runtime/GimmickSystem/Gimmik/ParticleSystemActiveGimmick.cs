using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// ParticleSystemのアクティブコントロールをするGimmick
    /// </summary>
    public class ParticleSystemActiveGimmick : ActiveGimmick {
        [SerializeField, Tooltip("Active制御する対象")]
        private ParticleSystem[] _targets;

        private readonly List<ParticleSystem> _particleSystems = new();

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            _particleSystems.Clear();

            // 全パーティクルの停止
            foreach (var ps in _targets) {
                if (ps == null) {
                    continue;
                }

                // ParticleSystemをキャッシュ
                _particleSystems.AddRange(ps.GetComponentsInChildren<ParticleSystem>(true));
            }
        }

        /// <inheritdoc/>
        protected override void SetSpeedInternal(float speed) {
            foreach (var ps in _particleSystems) {
                var main = ps.main;
                main.simulationSpeed = speed;
            }
        }

        /// <inheritdoc/>
        protected override void ActivateInternal(bool immediate) {
            foreach (var ps in _targets) {
                if (ps == null) {
                    continue;
                }

                ps.Play(true);
            }
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal(bool immediate) {
            foreach (var ps in _targets) {
                if (ps == null) {
                    continue;
                }

                if (immediate) {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                else {
                    ps.Stop(true);
                }
            }
        }
    }
}
