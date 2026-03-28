using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// Invoke時にComponentを再アクティブ化させるギミック
    /// </summary>
    public class ReenableInvokeGimmick : InvokeGimmick {
        [SerializeField, Tooltip("再生対象リスト")]
        private MonoBehaviour[] _targets;

        /// <inheritdoc/>
        protected override void InvokeInternal() {
            foreach (var target in _targets) {
                if (target == null) {
                    Debug.LogWarning($"Not found component. {gameObject.name}");
                    continue;
                }

                if (target.enabled) {
                    target.enabled = false;
                }

                target.enabled = true;
            }
        }
    }
}
