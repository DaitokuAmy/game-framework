using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// SceneをEntityと紐づけるためのComponent
    /// </summary>
    [Preserve]
    public sealed class SceneActorEntityComponent : ActorEntityComponent {
        /// <summary>シーン</summary>
        public Scene Scene { get; private set; } = default;

        /// <summary>
        /// Sceneの設定
        /// </summary>
        /// <param name="scene">設定するScene</param>
        /// <param name="prevDispose">既に設定されているSceneをRemoveするか</param>
        public ActorEntity SetScene(Scene scene, bool prevDispose = true) {
            if (Scene.IsValid()) {
                if (prevDispose) {
                    SceneManager.UnloadSceneAsync(Scene);
                }
            }

            Scene = scene;
            return Entity;
        }

        /// <summary>
        /// Sceneの削除
        /// </summary>
        /// <param name="dispose">SceneをDisposeするか</param>
        public ActorEntity RemoveScene(bool dispose = true) {
            return SetScene(default, dispose);
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            RemoveScene();
        }
    }
}