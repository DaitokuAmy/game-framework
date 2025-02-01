using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Invoke制御するParticleSystem再生用ギミック
    /// </summary>
    public class ParticleSystemInvokeGimmick : InvokeGimmick {
        [SerializeField, Tooltip("再生対象のParticleSystemリスト")]
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
        /// 実行処理
        /// </summary>
        protected override void InvokeInternal() {
            foreach (var target in _targets) {
                if (target == null) {
                    Debug.LogWarning($"Not found particle system. {gameObject.name}");
                    continue;
                }

                target.Play(true);
            }
        }
    }
}