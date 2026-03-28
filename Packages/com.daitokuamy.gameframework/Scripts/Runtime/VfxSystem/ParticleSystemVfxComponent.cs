using UnityEngine;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// ParticleSystem制御用のVfxComponent
    /// </summary>
    public sealed class ParticleSystemVfxComponent : IVfxComponent {
        // 再生基点のParticleSystem
        private ParticleSystem _rootParticleSystem;
        // 含まれているParticleSystem
        private ParticleSystem[] _particleSystems;

        /// <inheritdoc/>
        bool IVfxComponent.IsPlaying => _rootParticleSystem != null && _rootParticleSystem.IsAlive(true);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="particleSystem">制御対象のParticleSystem</param>
        public ParticleSystemVfxComponent(ParticleSystem particleSystem) {
            _rootParticleSystem = particleSystem;
            if (_rootParticleSystem != null) {
                _particleSystems = _rootParticleSystem.GetComponentsInChildren<ParticleSystem>(true);
            }
        }

        /// <inheritdoc/>
        void IVfxComponent.Tick(float deltaTime) {
        }

        /// <inheritdoc/>
        void IVfxComponent.Play() {
            if (_rootParticleSystem == null) {
                return;
            }

            _rootParticleSystem.Play(true);
        }

        /// <inheritdoc/>
        void IVfxComponent.Stop() {
            if (_rootParticleSystem == null) {
                return;
            }

            _rootParticleSystem.Stop(true);
        }

        /// <inheritdoc/>
        void IVfxComponent.StopImmediate() {
            if (_rootParticleSystem == null) {
                return;
            }

            _rootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <inheritdoc/>
        void IVfxComponent.SetSpeed(float speed) {
            if (_rootParticleSystem == null) {
                return;
            }

            for (var i = 0; i < _particleSystems.Length; i++) {
                var ps = _particleSystems[i];
                var main = ps.main;
                main.simulationSpeed = speed;
            }
        }

        /// <inheritdoc/>
        void IVfxComponent.SetLodLevel(int level) {
        }
    }
}
