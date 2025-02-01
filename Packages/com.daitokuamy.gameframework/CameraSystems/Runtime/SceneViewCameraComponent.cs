using Unity.Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.CameraSystems {
    /// <summary>
    /// シーンビューと同期するカメラコンポーネント
    /// </summary>
    public class SceneViewCameraComponent : SerializedCameraComponent<CinemachineCamera> {
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // 基本的なコンポーネントは削除する
            var bodyComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (bodyComponent != null) {
                Destroy(bodyComponent);
            }

            var aimComponent = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            if (aimComponent != null) {
                Destroy(aimComponent);
            }
        }

        /// <summary>
        /// カメラ更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
#if UNITY_EDITOR
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) {
                // 姿勢をトレースする
                var sceneViewCamera = sceneView.camera;
                var sceneViewCameraTrans = sceneViewCamera.transform;
                var trans = VirtualCamera.transform;
                trans.position = sceneViewCameraTrans.position;
                trans.rotation = sceneViewCameraTrans.rotation;
                VirtualCamera.Lens.FieldOfView = sceneViewCamera.fieldOfView;
            }
#endif
        }
    }
}