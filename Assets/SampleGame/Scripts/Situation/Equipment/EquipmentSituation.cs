using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;

namespace SampleGame {
    /// <summary>
    /// 装備画面シチュエーション
    /// </summary>
    public class EquipmentSituation : Situation {        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EquipmentSituation() {
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
            
            // UI読み込み
            var uIManager = Services.Get<UIManager>();
            var uiLoadHandle = uIManager.LoadSceneAsync("ui_equipment");
            uiLoadHandle.ScopeTo(scope);
            yield return uiLoadHandle;
        }
    }
}