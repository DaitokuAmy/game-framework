using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// Field用のHudSituation
    /// </summary>
    public class FieldHudNodeSituation : FieldNodeSituation {
        private SituationTree _situationTree;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldHudNodeSituation() {
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
            Debug.Log($"Load completed {GetType().Name}");
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);
            Debug.Log($"Setup completed {GetType().Name}");
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.P)) {
                Transition<EquipmentTopNodeSituation>();
            }
        }
    }
}