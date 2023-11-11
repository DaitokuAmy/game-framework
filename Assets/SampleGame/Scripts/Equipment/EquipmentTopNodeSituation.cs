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
        private SituationFlow _situationFlow;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EquipmentTopNodeSituation() {
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

            var window = Services.Get<UIManager>().GetWindow<EquipmentUIWindow>();
            window.BackButton.OnClickSubject
                .TakeUntil(scope)
                .Subscribe(_ => Back());
            
            window.TransitionTopAsync(CancellationToken.None).Forget();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            var window = Services.Get<UIManager>().GetWindow<EquipmentUIWindow>();

            if (Input.GetKeyDown(KeyCode.P)) {
                Back();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                window.TransitionTopAsync(CancellationToken.None).Forget();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                window.TransitionArmorListAsync(CancellationToken.None).Forget();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                window.TransitionWeaponListAsync(CancellationToken.None).Forget();
            }
        }
    }
}