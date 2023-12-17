using System.Collections;
using GameFramework.CollisionSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体の実体制御用MonoBehaviour
    /// </summary>
    public class ParticleBulletProjectileComponent : BulletProjectileComponent {
        [SerializeField, Tooltip("飛翔中に再生するParticleSystem(Loop)")]
        private ParticleSystem _baseParticle;
        [SerializeField, Tooltip("ヒット時再生するParticleSystem(OneShot)")]
        private ParticleSystem _hitParticle;
        [SerializeField, Tooltip("終了時再生するParticleSystem(OneShot)")]
        private ParticleSystem _exitParticle;

        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        protected override void SetSpeedInternal(float speed) {
            SetSpeedParticle(_baseParticle, speed);
            SetSpeedParticle(_hitParticle, speed);
            SetSpeedParticle(_exitParticle, speed);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected override void StartProjectileInternal() {
            StopParticle(_hitParticle);
            StopParticle(_exitParticle);

            PlayParticle(_baseParticle);
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected override IEnumerator ExitProjectileRoutine() {
            StopParticle(_baseParticle);
            PlayParticle(_exitParticle);

            // Particleの再生が完了するまで待つ
            while (true) {
                if (IsAliveParticle(_baseParticle) || IsAliveParticle(_hitParticle) || IsAliveParticle(_exitParticle)) {
                    yield return null;
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// コリジョンヒット通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        protected override void OnHitCollisionInternal(RaycastHitResult result) {
            PlayParticle(_hitParticle);
        }

        /// <summary>
        /// Particleを再生
        /// </summary>
        private void PlayParticle(ParticleSystem particle) {
            if (particle == null) {
                return;
            }

            particle.Play();
        }

        /// <summary>
        /// Particleを停止
        /// </summary>
        private void StopParticle(ParticleSystem particle) {
            if (particle == null) {
                return;
            }

            particle.Stop();
        }

        /// <summary>
        /// Particleが生存中か
        /// </summary>
        private bool IsAliveParticle(ParticleSystem particle) {
            if (particle == null) {
                return false;
            }

            return particle.IsAlive(true);
        }

        /// <summary>
        /// Particleの速度変更
        /// </summary>
        private void SetSpeedParticle(ParticleSystem particle, float speed) {
            if (particle == null) {
                return;
            }

            var main = particle.main;
            main.simulationSpeed = speed;
        }
    }
}