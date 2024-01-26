using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// Gimmickを管理するルートコンポーネント
    /// </summary>
    public class GimmickRoot : MonoBehaviour {
        /// <summary>
        /// 設定読み込み時処理
        /// </summary>
        private void OnValidate() {
            gameObject.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;
        }
    }
}