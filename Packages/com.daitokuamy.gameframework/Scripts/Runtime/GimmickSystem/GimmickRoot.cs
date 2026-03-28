using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// Gimmickを管理するルートコンポーネント
    /// </summary>
    public sealed class GimmickRoot : MonoBehaviour {
        /// <summary>
        /// 設定読み込み時処理
        /// </summary>
        private void OnValidate() {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
    }
}
