using UnityEngine;

namespace GameFramework.GimmickSystems {
    /// <summary>
    /// GameObjectのアクティブコントロールをするGimmick
    /// </summary>
    public class GameObjectActiveGimmick : ActiveGimmick {
        [SerializeField, Tooltip("Active制御する対象")]
        private GameObject[] _targets;

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        protected override void ActivateInternal(bool immediate) {
            foreach (var obj in _targets) {
                if (obj == null) {
                    continue;
                }

                obj.SetActive(true);
            }
        }

        /// <summary>
        /// 非アクティブ化処理
        /// </summary>
        protected override void DeactivateInternal(bool immediate) {
            foreach (var obj in _targets) {
                if (obj == null) {
                    continue;
                }

                obj.SetActive(false);
            }
        }
    }
}