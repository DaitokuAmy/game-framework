using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.CameraSystems {
    /// <summary>
    /// シーンビューと同期するカメラコンポーネント
    /// </summary>
    public class SceneViewCameraComponent : SerializedCameraComponent<CinemachineVirtualCamera> {
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            // 基本的なコンポーネントは削除する
            VirtualCamera.DestroyCinemachineComponent<CinemachineComponentBase>();
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
                VirtualCamera.m_Lens.FieldOfView = sceneViewCamera.fieldOfView;
            }
#endif
        }
    }
}