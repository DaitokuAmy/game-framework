using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Battle;
using UniRx;

namespace SampleGame.Battle {
    /// <summary>
    /// Battle用のSceneSituation
    /// </summary>
    public class BattleSceneSituation : SceneSituation {
        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/battle.unity";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
            
            // UI読み込み
            var tasks = new List<UniTask>();
            tasks.Add(LoadUIAsync(scope, scope.Token));

            yield return UniTask.WhenAll(tasks).ToCoroutine();
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.OpenRoutineInternal(handle, animationScope);
            
            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.OpenAsync();
        }

        /// <summary>
        /// 開いた後の処理
        /// </summary>
        protected override void PostOpenInternal(TransitionHandle handle, IScope scope) {
            base.PostOpenInternal(handle, scope);
            
            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.OpenAsync(immediate: true);
        }

        /// <summary>
        /// 閉く処理
        /// </summary>
        protected override IEnumerator CloseRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield return base.CloseRoutineInternal(handle, animationScope);
            
            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            yield return battleHudUIService.BattleHudUIScreen.CloseAsync();
        }

        /// <summary>
        /// 閉じた後の処理
        /// </summary>
        protected override void PostCloseInternal(TransitionHandle handle) {
            base.PostCloseInternal(handle);
            
            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();
            battleHudUIService.BattleHudUIScreen.CloseAsync(immediate: true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
            base.ActivateInternal(handle, scope);

            var situationService = Services.Get<SituationService>();
            var uiManager = Services.Get<UIManager>();
            var battleHudUIService = uiManager.GetService<BattleHudUIService>();

            // メニューボタン
            battleHudUIService.BattleHudUIScreen.ClickedMenuButtonSubject
                .TakeUntil(scope)
                .Subscribe(_ => {
                    situationService.Transition<BattlePauseSituation>();
                });
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            var uiManager = Services.Get<UIManager>();

            UniTask LoadAsync(string assetKey) {
                return uiManager.LoadSceneAsync(assetKey).ScopeTo(unloadScope).ToUniTask(cancellationToken: ct);
            }
            
            return UniTask.WhenAll(LoadAsync("ui_battle"));
        }
    }
}