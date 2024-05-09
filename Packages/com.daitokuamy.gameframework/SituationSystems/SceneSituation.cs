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
        /// <summary>シーンのアセットパス</summary>
        protected abstract string SceneAssetPath { get; }
        /// <summary>アンロード時の空シーンのアセットパス(未指定だとアンロードでシーンを廃棄しない)</summary>
        protected virtual string EmptySceneAssetPath { get; } = "";

        /// <summary>空シーンが指定されているか</summary>
        public bool HasEmptyScene => !string.IsNullOrEmpty(EmptySceneAssetPath);
        /// <summary>シーン情報</summary>
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
        /// アンロード処理
        /// </summary>
        protected override void UnloadInternal(TransitionHandle handle) {
            // シーンの解放
            if (HasEmptyScene && Scene.IsValid()) {
                SceneManager.LoadScene(EmptySceneAssetPath);
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

        /// <summary>
        /// 遷移用のデフォルトTransition取得
        /// </summary>
        public override ITransition GetDefaultNextTransition() {
            return new OutInTransition(backgroundThreadPriority: ThreadPriority.High);
        }
    }
}