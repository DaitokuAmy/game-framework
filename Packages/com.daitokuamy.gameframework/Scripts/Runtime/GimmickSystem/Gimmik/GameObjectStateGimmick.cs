using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// GameObjectを使ったStateGimmick
    /// </summary>
    public class GameObjectStateGimmick : StateGimmickBase<GameObjectStateGimmick.StateInfo> {
        /// <summary>
        /// ステート情報基底
        /// </summary>
        [Serializable]
        public class StateInfo : StateInfoBase {
            [Tooltip("アクティブにするターゲットリスト")]
            public GameObject[] activeTargets;
        }

        private GameObject[] _allTargets;

        /// <inheritdoc/>
        protected override void InitializeInternal() {
            base.InitializeInternal();

            // 全部のステートに含まれるTargetを列挙
            var allTargets = new HashSet<GameObject>();
            foreach (var stateInfo in StateInfos) {
                foreach (var target in stateInfo.activeTargets) {
                    if (target == null) {
                        continue;
                    }

                    allTargets.Add(target);
                }
            }

            _allTargets = allTargets.ToArray();
        }

        /// <inheritdoc/>
        protected override void ChangeState(StateInfo prev, StateInfo current, bool immediate) {
            foreach (var target in _allTargets) {
                target.SetActive(current != null && current.activeTargets.Contains(target));
            }
        }
    }
}
