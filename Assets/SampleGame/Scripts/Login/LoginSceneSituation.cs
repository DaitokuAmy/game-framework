using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;
using Input = UnityEngine.Input;

namespace SampleGame {
    /// <summary>
    /// Login用のSituation
    /// </summary>
    public class LoginSceneSituation : SceneSituation {
        // ログイン後に遷移するシチュエーション
        private SceneSituation _nextSceneSituation;

        private float _timer;

        protected override string SceneAssetPath => "login";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="nextSceneSituation">ログイン後に遷移するシチュエーション</param>
        public LoginSceneSituation(SceneSituation nextSceneSituation) {
            _nextSceneSituation = nextSceneSituation;
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.LoadRoutineInternal(handle, scope);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield return base.SetupRoutineInternal(handle, scope);
            
            _timer = 1.5f;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            _timer -= Time.deltaTime;

            if (_timer <= 0.0f) {
                ParentContainer.Transition(_nextSceneSituation);
            }
        }
    }
}