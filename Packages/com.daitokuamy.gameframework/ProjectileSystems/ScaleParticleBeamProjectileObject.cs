using System.Collections;
using GameFramework.CollisionSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 拡大を使ったビームオブジェクト制御
    /// </summary>
    public class ScaleParticleBeamProjectileObject : BeamProjectileObject {
        [SerializeField, Tooltip("拡縮可能なObject")]
        private GameObject _scalableObject;
        [SerializeField, Tooltip("拡縮可能ObjectのZ方向基準サイズ")]
        private float _scalableObjectSizeZ = 1.0f;
        [SerializeField, Tooltip("先端のパーティクル(Loop)")]
        private ParticleSystem _headParticle;
        [SerializeField, Tooltip("末端のパーティクル(Loop)")]
        private ParticleSystem _tailParticle;
        [SerializeField, Tooltip("衝突点のパーティクル(Loop)")]
        private ParticleSystem _collisionParticle;
        [SerializeField, Tooltip("ヒット時パーティクル(OneShot)")]
        private ParticleSystem _hitParticle;
        [SerializeField, Tooltip("先端の終了時パーティクル(OneShot)")]
        private ParticleSystem _exitHeadParticle;
        
        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected override void StartProjectileInternal() {
            StopParticle(_hitParticle);
            StopParticle(_exitHeadParticle);
            StopParticle(_collisionParticle);

            PlayParticle(_headParticle);
            PlayParticle(_tailParticle);
        }
        
        /// <summary>
        /// 内部用Transform更新処理
        /// </summary>
        protected override void UpdateTransformInternal(IBeamProjectile projectile) {
            if (_headParticle != null) {
                _headParticle.transform.position = projectile.HeadPosition;
            }

            if (_tailParticle != null) {
                _tailParticle.transform.position = projectile.TailPosition;
            }

            if (_collisionParticle != null) {
                if (projectile.IsHitting) {
                    _collisionParticle.transform.position = projectile.HeadPosition;
                    PlayParticle(_collisionParticle);
                }
                else {
                    StopParticle(_collisionParticle);
                }
            }

            if (_scalableObject != null) {
                var distance = projectile.Distance;
                var scale = new Vector3(1.0f, 1.0f, distance / _scalableObjectSizeZ / transform.localScale.z);
                _scalableObject.transform.localScale = scale;
            }
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected override IEnumerator ExitProjectileRoutine() {
            StopParticle(_headParticle);
            StopParticle(_tailParticle);
            PlayParticle(_exitHeadParticle);

            // Particleの再生が完了するまで待つ
            while (true) {
                if (IsAliveParticle(_headParticle) || IsAliveParticle(_tailParticle) || IsAliveParticle(_hitParticle) || IsAliveParticle(_exitHeadParticle)) {
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
        protected override void OnHitCollision(RaycastHitResult result) {
            PlayParticle(_hitParticle);
        }

        /// <summary>
        /// Particleを再生
        /// </summary>
        private void PlayParticle(ParticleSystem particle) {
            if (particle == null) {
                return;
            }

            if (particle.isPlaying) {
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

            if (!particle.isPlaying) {
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
    }
}