using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Equipment;
using UnityEngine;
using UniRx;

namespace SampleGame {
    /// <summary>
    /// EquipmentTopNodeSituation
    /// </summary>
    public class EquipmentTopNodeSituation : FieldNodeSituation {        
        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);

            var uiManager = Services.Get<UIManager>();
            var uiService = uiManager.GetService<EquipmentUIService>();
            yield return uiService.ChangeTopAsync(CancellationToken.None).ToCoroutine();
        }

        /// <summary>
        /// 開く後処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle handle, IScope scope) {
            base.PostOpenInternal(handle, scope);
            
            var uiManager = Services.Get<UIManager>();
            var uiService = uiManager.GetService<EquipmentUIService>();
            uiService.ChangeTopImmediate();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);
            var uiManager = Services.Get<UIManager>();
            var uiService = uiManager.GetService<EquipmentUIService>();
            
            // 戻る
            uiService.BackButton.OnClickSubject
                .TakeUntil(scope)
                .Subscribe(_ => Back());

            // 各種画面遷移
            uiService.TopScreen.WeaponUIView.OnClickSubject
                .TakeUntil(scope)
                .Subscribe(_ => Transition<EquipmentWeaponListNodeSituation>());
            
            uiService.TopScreen.HelmArmorUIView.OnClickSubject
                .TakeUntil(scope)
                .Subscribe(_ => Transition<EquipmentArmorListNodeSituation>(overrideTransition:new OutInTransition(true, true)));
            
            uiService.TopScreen.BodyArmorUIView.OnClickSubject
                .TakeUntil(scope)
                .Subscribe(_ => Transition<EquipmentArmorListNodeSituation>());
            
            uiService.TopScreen.ArmsArmorUIView.OnClickSubject
                .TakeUntil(scope)
                .Subscribe(_ => Transition<EquipmentArmorListNodeSituation>());
            
            uiService.TopScreen.LegsArmorUIView.OnClickSubject
                .TakeUntil(scope)
                .Subscribe(_ => Transition<EquipmentArmorListNodeSituation>());
        }
    }
}