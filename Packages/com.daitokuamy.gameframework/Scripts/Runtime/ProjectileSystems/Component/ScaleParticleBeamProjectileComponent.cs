using System;
using System.Collections;
using GameFramework.CollisionSystems;
using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 拡大を使ったビームオブジェクト制御
    /// </summary>
    public class ScaleParticleBeamProjectileComponent : BeamProjectileComponent {
        /// <summary>
        /// 軸タイプ
        /// </summary>
        private enum AxisType {
            X,
            Y,
            Z,
        }

        /// <summary>
        /// スケール可能オブジェクト情報
        /// </summary>
        [Serializable]
        private class ScalableObjectInfo {
            public GameObject target;
            public AxisType axisType = AxisType.Z;
            public float baseScale = 1.0f;
        }

        /// <summary>
        /// スケール可能オブジェクト情報
        /// </summary>
        [Serializable]
        private class ScalableParticleInfo {
            public ParticleSystem target;
            public AxisType axisType = AxisType.Z;
            public float baseScale = 1.0f;
            public float basePosition = -0.5f;
        }

        [SerializeField, Tooltip("拡縮可能オブジェクト情報リスト")]
        private ScalableObjectInfo[] _scalableObjectInfos;
        [SerializeField, Tooltip("拡縮可能パーティクル情報リスト")]
        private ScalableParticleInfo[] _scalableParticleInfos;
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

        [SerializeField, Tooltip("ヒット時に再生するParticleSystemがヒット位置に出るか")]
        private bool _fitHitParticle = true;
        [SerializeField, Tooltip("ヒット時に再生するParticleSystemがヒット法線方向に向くか")]
        private bool _rotateNormalHitParticle = true;

        /// <summary>
        /// 再生速度の変更
        /// </summary>
        /// <param name="speed">1.0を基準とした速度</param>
        protected override void SetSpeedInternal(float speed) {
            foreach (var info in _scalableParticleInfos) {
                SetSpeedParticle(info.target, speed);
            }

            SetSpeedParticle(_headParticle, speed);
            SetSpeedParticle(_tailParticle, speed);
            SetSpeedParticle(_collisionParticle, speed);
            SetSpeedParticle(_hitParticle, speed);
            SetSpeedParticle(_exitHeadParticle, speed);
        }

        /// <summary>
        /// 飛翔開始処理
        /// </summary>
        protected override void StartInternal() {
            StopParticle(_hitParticle);
            StopParticle(_exitHeadParticle);
            StopParticle(_collisionParticle);

            PlayParticle(_headParticle);
            PlayParticle(_tailParticle);
            foreach (var info in _scalableParticleInfos) {
                PlayParticle(info.target);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);

            UpdateTransformInternal(ProjectileController);
        }

        /// <summary>
        /// 飛翔終了子ルーチン処理
        /// </summary>
        protected override IEnumerator ExitRoutineInternal() {
            UpdateTransformInternal(ProjectileController);

            StopParticle(_headParticle);
            StopParticle(_tailParticle);
            foreach (var info in _scalableParticleInfos) {
                StopParticle(info.target);
            }

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
        /// コリジョンヒット時通知
        /// </summary>
        /// <param name="hit">当たり結果</param>
        protected override void OnHitCollisionInternal(RaycastHit hit) {
            if (_fitHitParticle) {
                var trans = _hitParticle.transform;
                trans.position = hit.point;
                if (_rotateNormalHitParticle) {
                    trans.rotation = Quaternion.LookRotation(hit.normal);
                }
            }

            PlayParticle(_hitParticle);
        }

        /// <summary>
        /// 内部用Transform更新処理
        /// </summary>
        private void UpdateTransformInternal(IBeamProjectileController projectileController) {
            if (_headParticle != null) {
                _headParticle.transform.position = projectileController.HeadPosition;
            }

            if (_tailParticle != null) {
                _tailParticle.transform.position = projectileController.TailPosition;
            }

            if (_collisionParticle != null) {
                if (projectileController.IsHitting) {
                    _collisionParticle.transform.position = projectileController.HeadPosition;
                    PlayParticle(_collisionParticle);
                }
                else {
                    StopParticle(_collisionParticle);
                }
            }

            SetScalableDistance(ProjectileController.Distance);
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

        /// <summary>
        /// Scalable情報の距離を設定
        /// </summary>
        private void SetScalableDistance(float distance) {
            foreach (var info in _scalableObjectInfos) {
                if (info.target == null) {
                    continue;
                }

                if (info.baseScale <= 0.001f) {
                    continue;
                }

                var scale = info.target.transform.localScale;
                scale[(int)info.axisType] = distance / info.baseScale / transform.localScale.z;
                info.target.transform.localScale = scale;
            }

            foreach (var info in _scalableParticleInfos) {
                if (info.target == null) {
                    continue;
                }

                if (info.baseScale <= 0.001f) {
                    continue;
                }

                var shape = info.target.shape;
                var scale = shape.scale;
                var position = shape.position;
                var index = (int)info.axisType;
                scale[index] = distance / info.baseScale / transform.localScale.z;
                position[index] = shape.scale[index] * info.basePosition;
                shape.scale = scale;
                shape.position = position;
            }
        }
    }
}