using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Field;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// Field用のHudSituation
    /// </summary>
    public class FieldHudNodeSituation : FieldNodeSituation {
        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);
            var uiManager = Services.Get<UIManager>();
            var hudWindow = uiManager.GetWindow<FieldHudUIWindow>();
            
            // 装備画面への遷移
            hudWindow.FooterUIView.OnClickEquipmentButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    Transition<EquipmentTopNodeSituation>();
                });
        }
    }
}