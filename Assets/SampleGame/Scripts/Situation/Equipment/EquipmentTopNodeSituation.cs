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
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            var uiService = Services.Get<UIManager>().GetService<EquipmentUIService>();
            uiService.ChangeTopAsync(CancellationToken.None).Forget();
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
                .Subscribe(_ => Transition<EquipmentArmorListNodeSituation>());
            
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