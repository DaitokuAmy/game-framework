using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameFramework.Core;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 加算シーン遷移に使うシチュエーション
    /// </summary>
    public abstract class AdditiveSceneSituation : Situation {
        /// <summary>UnitySceneを保持するSituationか</summary>
        public override bool HasScene => true;
        
        /// <summary>シーンのアセットパス</summary>
        protected abstract string SceneAssetPath { get; }

        /// <summary>シーン情報</summary>
        protected Scene Scene { get; private set; }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            // シーンの切り替え
            yield return SceneManager.LoadSceneAsync(SceneAssetPath, LoadSceneMode.Additive);

            // シーンの取得
            Scene = SceneManager.GetSceneByPath(SceneAssetPath);

            if (Scene.path != SceneAssetPath && Scene.name != SceneAssetPath) {
                Debug.LogError($"Failed load scene. [{SceneAssetPath}]");
            }

            // Serviceのインストール
            var installers = Scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<ServiceContainerInstaller>(true))
                .ToArray();
            foreach (var installer in installers) {
                installer.Install(ServiceContainer, scope);
            }
        }

        /// <summary>
        /// アンロード処理
        /// </summary>
        protected override void UnloadInternal(TransitionHandle handle) {
            // シーンの解放
            if (Scene.IsValid()) {
                SceneManager.UnloadSceneAsync(Scene);
            }
        }
    }
}