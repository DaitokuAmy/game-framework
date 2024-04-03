using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// ParticleSystemのアクティブコントロールをするGimmick
    /// </summary>
    public class ParticleSystemActiveGimmick : ActiveGimmick {
        [SerializeField, Tooltip("Active制御する対象")]
        private ParticleSystem[] _targets;

        private readonly List<ParticleSystem> _particleSystems = new();

        /// <summary>
        /// 初期化処理
        /// </summary>
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

        /// <summary>
        /// 速度の変更
        /// </summary>
        protected override void SetSpeedInternal(float speed) {
            foreach (var ps in _particleSystems) {
                var main = ps.main;
                main.simulationSpeed = speed;
            }
        }

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        protected override void ActivateInternal(bool immediate) {
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