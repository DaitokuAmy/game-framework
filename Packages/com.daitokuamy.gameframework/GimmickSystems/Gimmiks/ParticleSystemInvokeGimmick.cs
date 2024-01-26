using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Invoke制御するParticleSystem再生用ギミック
    /// </summary>
    public class ParticleSystemInvokeGimmick : InvokeGimmick {
        [SerializeField, Tooltip("再生対象のParticleSystemリスト")]
        private ParticleSystem[] _targets;

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