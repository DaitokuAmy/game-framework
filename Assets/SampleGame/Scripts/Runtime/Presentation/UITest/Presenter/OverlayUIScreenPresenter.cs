using GameFramework;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation.UITest {
    /// <summary>
    /// OverlayUIScreenのPresenter
    /// </summary>
    public class OverlayUIScreenPresenter : UIScreenLogic<UIScreenContainer> {
        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                Screen.Transition("Win");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                Screen.Transition("Lose");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                Screen.Clear();
            }
        }
    }
}