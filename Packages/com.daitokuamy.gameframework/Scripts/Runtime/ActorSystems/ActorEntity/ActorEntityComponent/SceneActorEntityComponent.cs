using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// SceneをEntityと紐づけるためのComponent
    /// </summary>
    [Preserve]
    public sealed class SceneActorEntityComponent : ActorEntityComponent {
        private bool _autoDispose;
        
        /// <summary>シーン</summary>
        public Scene Scene { get; private set; } = default;

        /// <summary>
        /// Sceneの設定
        /// </summary>
        /// <param name="scene">設定するScene</param>
        /// <param name="autoDispose">Dispose時に自動削除するか</param>
        public ActorEntity SetScene(Scene scene, bool autoDispose = true) {
            if (Scene.IsValid()) {
                if (_autoDispose) {
                    SceneManager.UnloadSceneAsync(Scene);
                }
            }

            Scene = scene;
            _autoDispose = autoDispose;
            return Entity;
        }

        /// <summary>
        /// Sceneの削除
        /// </summary>
        public ActorEntity RemoveScene() {
            return SetScene(default, false);
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            RemoveScene();
        }
    }
}