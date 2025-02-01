using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Invoke時にGameObjectを再アクティブ化させるギミック
    /// </summary>
    public class ReactivateInvokeGimmick : InvokeGimmick {
        [SerializeField, Tooltip("再生対象リスト")]
        private GameObject[] _targets;

        /// <summary>
        /// 実行処理
        /// </summary>
        protected override void InvokeInternal() {
            foreach (var target in _targets) {
                if (target == null) {
                    Debug.LogWarning($"Not found gameObject. {gameObject.name}");
                    continue;
                }

                if (target.activeSelf) {
                    target.SetActive(false);
                }

                target.SetActive(true);
            }
        }
    }
}