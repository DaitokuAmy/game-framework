using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;
using Input = UnityEngine.Input;

namespace SampleGame {
    /// <summary>
    /// Title用のSituation
    /// </summary>
    public class TitleSceneSituation : SceneSituation {
        // TitleScene内シチュエーション用コンテナ
        private SituationContainer _situationContainer;

        protected override string SceneAssetPath => "title";

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            if (Input.GetKeyDown(KeyCode.Space)) {
                ParentContainer.Transition(new FieldSceneSituation(), new FadeTransitionEffect(Color.black, 0.5f));
            }
        }
    }
}