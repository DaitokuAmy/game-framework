using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// ParticleSystemを使ったStateGimmick
    /// </summary>
    public class ParticleSystemStateGimmick : StateGimmickBase<ParticleSystemStateGimmick.StateInfo> {
        /// <summary>
        /// ステート情報基底
        /// </summary>
        [Serializable]
        public class StateInfo : StateInfoBase {
            [Tooltip("再生するParticleSystem")]
            public ParticleSystem[] activeParticleSystems;
        }

        private readonly List<ParticleSystem> _particleSystems = new();

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            base.InitializeInternal();
            
            _particleSystems.Clear();
            
            // 全パーティクルの停止
            foreach (var info in StateInfos) {
                foreach (var ps in info.activeParticleSystems) {
                    if (ps == null) {
                        continue;
                    }
                    
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    
                    // ParticleSystemをキャッシュ
                    _particleSystems.AddRange(ps.GetComponentsInChildren<ParticleSystem>(true));
                }
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
        protected override void ChangeState(StateInfo prev, StateInfo current, bool immediate) {
            if (prev != null) {
                foreach (var ps in prev.activeParticleSystems) {
                    if (ps == null) {
                        continue;
                    }
                    
                    ps.Stop(true, immediate ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
                }
            }
            
            if (current != null) {
                foreach (var ps in current.activeParticleSystems) {
                    if (ps == null) {
                        continue;
                    }
                    
                    ps.Play(true);
                }
            }
        }
    }
}
