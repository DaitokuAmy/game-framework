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
        /// <summary>UnitySceneを保持するSituationか</summary>
        public override bool HasScene => true;
        /// <summary>PreLoad可能か</summary>
        public override bool CanPreLoad => false;

        /// <summary>シーンのアセットパス</summary>
        protected abstract string SceneAssetPath { get; }
        /// <summary>アンロード時の空シーンのアセットパス(未指定だとアンロードでシーンを廃棄しない)</summary>
        protected virtual string EmptySceneAssetPath { get; } = "";

        /// <summary>空シーンが指定されているか</summary>
        public bool HasEmptyScene => !string.IsNullOrEmpty(EmptySceneAssetPath);

        /// <summary>シーン情報</summary>
        protected Scene Scene { get; private set; }

        /// <summary>
        /// 親要素として適切かチェックする
        /// </summary>
        protected override bool CheckParent(ISituation parent) {
            // 親にSceneを持ったSituationがある場合は許可しない
            var p = parent;
            while (p != null) {
                if (p.HasScene) {
                    return false;
                }

                p = p.Parent;
            }

            return base.CheckParent(parent);
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        protected override IEnumerator LoadRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            // シーンの切り替え
            yield return SceneManager.LoadSceneAsync(SceneAssetPath, LoadSceneMode.Single);

            // シーンの取得
            Scene = SceneManager.GetActiveScene();

            if (Scene.path != SceneAssetPath && Scene.name != SceneAssetPath) {
                Debug.LogError($"Failed load scene. [{SceneAssetPath}]");
            }

            var rootObjects = Scene.GetRootGameObjects();
            // Serviceのインストール
            var installers = rootObjects
                .SelectMany(x => x.GetComponentsInChildren<ServiceContainerInstaller>(true))
                .ToArray();
            foreach (var installer in installers) {
                installer.Install(ServiceContainer, scope);
            }

            // ServiceUserへのResolver提供
            var users = rootObjects
                .SelectMany(x => x.GetComponentsInChildren<IServiceUser>(true))
                .ToArray();
            foreach (var user in users) {
                user.ImportService(ServiceResolver);
            }
        }

        /// <summary>
        /// アンロード処理
        /// </summary>
        protected override void UnloadInternal(TransitionHandle<Situation> handle) {
            // シーンの解放
            if (HasEmptyScene && Scene.IsValid()) {
                SceneManager.LoadScene(EmptySceneAssetPath);
            }
        }

        /// <summary>
        /// 遷移用のデフォルトTransition取得
        /// </summary>
        public override ITransition GetDefaultNextTransition() {
            return new OutInTransition(backgroundThreadPriority: ThreadPriority.High);
        }
    }
}