using UnityEngine;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// ParticleSystem制御用のVfxComponent
    /// </summary>
    public class ParticleSystemVfxComponent : IVfxComponent {
        // 再生基点のParticleSystem
        private ParticleSystem _rootParticleSystem;
        // 含まれているParticleSystem
        private ParticleSystem[] _particleSystems;

        // 再生中か
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

        /// <summary>
        /// 更新処理
        /// </summary>
        void IVfxComponent.Update(float deltaTime) {
        }

        /// <summary>
        /// 再生
        /// </summary>
        void IVfxComponent.Play() {
            if (_rootParticleSystem == null) {
                return;
            }

            _rootParticleSystem.Play(true);
        }

        /// <summary>
        /// 停止
        /// </summary>
        void IVfxComponent.Stop() {
            if (_rootParticleSystem == null) {
                return;
            }

            _rootParticleSystem.Stop(true);
        }

        /// <summary>
        /// 即時停止
        /// </summary>
        void IVfxComponent.StopImmediate() {
            if (_rootParticleSystem == null) {
                return;
            }

            _rootParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
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
    }
}