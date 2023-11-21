using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameFramework.Core;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シーン遷移に使うシチュエーション
    /// </summary>
    public abstract class SceneSituation : Situation {
        // シーンのアセットパス
        protected abstract string SceneAssetPath { get; }

        // シーン情報
        protected Scene Scene { get; private set; }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            // シーンの切り替え
            yield return SceneManager.LoadSceneAsync(SceneAssetPath, LoadSceneMode.Single);

            // シーンの取得
            Scene = SceneManager.GetActiveScene();

            if (Scene.path != SceneAssetPath && Scene.name != SceneAssetPath) {
                Debug.LogError($"Failed load scene. [{SceneAssetPath}]");
            }

            // Serviceのインストール
            var installers = Scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<ServiceContainerInstaller>(true))
                .ToArray();
            foreach (var installer in installers) {
                installer.Install(ServiceContainer);
            }
        }

        /// <summary>
        /// 遷移先チェック
        /// </summary>
        /// <param name="nextTransition">遷移するの子シチュエーション</param>
        /// <param name="transition">遷移処理</param>
        /// <returns>遷移可能か</returns>
        public override bool CheckNextTransition(Situation nextTransition, ITransition transition) {
            return nextTransition is SceneSituation && transition is OutInTransition;
        }
    }
}