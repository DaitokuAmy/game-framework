using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.SituationSystems;
using GameFramework.UISystems;
using SampleGame.Presentation.Introduction;
using UniRx;

namespace SampleGame.Introduction {
    /// <summary>
    /// Introduction用のSceneSituation
    /// </summary>
    public class IntroductionSceneSituation : SceneSituation {
        protected override string SceneAssetPath => "Assets/SampleGame/Scenes/introduction.unity";

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
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            var uiManager = Services.Get<UIManager>();

            UniTask LoadAsync(string assetKey) {
                return uiManager.LoadSceneAsync(assetKey).ScopeTo(unloadScope).ToUniTask(cancellationToken: ct);
            }
            
            return UniTask.WhenAll(LoadAsync("ui_introduction"));
        }
    }
}