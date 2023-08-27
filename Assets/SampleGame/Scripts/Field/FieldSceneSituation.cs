using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.BodySystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.CutsceneSystems;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Field;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// Field用のSituation
    /// </summary>
    public class FieldSceneSituation : SceneSituation {
        // FieldScene内シチュエーション用コンテナ
        private SituationContainer _situationContainer;

        protected override string SceneAssetPath => "field";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);

            var ct = scope.Token;
            
            // フィールド読み込み
            yield return new FieldSceneAssetRequest("fld001")
                .LoadAsync(true, scope, ct)
                .ToCoroutine();
            
            // UI読み込み
            var uIManager = Services.Get<UIManager>();
            var uiLoadHandle = uIManager.LoadSceneAsync("ui_field");
            uiLoadHandle.ScopeTo(scope);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);

            var ct = scope.Token;
            
            // CameraManagerの初期化
            var cameraManager = Services.Get<CameraManager>();
            cameraManager.RegisterTask(TaskOrder.Camera);
            
            // CutsceneManagerの生成
            var cutsceneManager = new CutsceneManager();
            ServiceContainer.Set(cutsceneManager);
            cutsceneManager.RegisterTask(TaskOrder.Cutscene);

            // BodyManagerの生成
            var bodyManager = new BodyManager();
            ServiceContainer.Set(bodyManager);
            bodyManager.RegisterTask(TaskOrder.Body);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.Space)) {
                ParentContainer.Transition(new BattleSceneSituation());
            }

            var uIManager = Services.Get<UIManager>();
            if (Input.GetKeyDown(KeyCode.O)) {
                uIManager.GetWindow<FieldHudUIWindow>().DailyDialogTestAsync().Forget();
            }

            if (Input.GetKeyDown(KeyCode.Alpha0)) {
                uIManager.GetWindow<FieldEquipmentUIWindow>().TransitionTopAsync(CancellationToken.None).Forget();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                uIManager.GetWindow<FieldEquipmentUIWindow>().TransitionWeaponListAsync(CancellationToken.None).Forget();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                uIManager.GetWindow<FieldEquipmentUIWindow>().TransitionArmorListAsync(CancellationToken.None).Forget();
            }
            if (Input.GetKeyDown(KeyCode.B)) {
                uIManager.GetWindow<FieldEquipmentUIWindow>().BackAsync(CancellationToken.None).Forget();
            }
        }
    }
}