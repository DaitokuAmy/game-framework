using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.BodySystems {
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

        /// <summary>
        /// 初期化処理
        /// </summary>
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

        /// <summary>
        /// ステートの変更処理
        /// </summary>
        /// <param name="prev">変更前のステート</param>
        /// <param name="current">変更後のステート</param>
        /// <param name="immediate">即時遷移するか</param>
        protected override void ChangeState(StateInfo prev, StateInfo current, bool immediate) {
            foreach (var target in _allTargets) {
                target.SetActive(current != null && current.activeTargets.Contains(target));
            }
        }
    }
}