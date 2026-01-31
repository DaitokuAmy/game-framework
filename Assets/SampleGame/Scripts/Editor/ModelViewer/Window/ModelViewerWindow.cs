using GameFramework;
using GameFramework.DebugSystems.Editor;
using SampleGame.Lifecycle;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// モデルビューア用のWindow
    /// </summary>
    public partial class ModelViewerWindow : DebugWindowBase<ModelViewerWindow> {
        /// <summary>VContainerインスタンスアクセス用</summary>
        private IObjectResolver Resolver => ModelViewerDebugObjectResolver.Instance;

        /// <summary>
        /// 開く処理
        /// </summary>
        [MenuItem("Window/Sample Game/Model Viewer")]
        private static void Open() {
            GetWindow<ModelViewerWindow>("Model Viewer");
        }

        /// <inheritdoc/>
        protected override void OnEnableInternal() {
            AddPanel(new ActorPanel());
            AddPanel(new BodyPanel());
            AddPanel(new AvatarPanel());
            AddPanel(new EnvironmentPanel());
            AddPanel(new RecordingPanel());
            AddPanel(new SettingsPanel());
        }

        /// <inheritdoc/>
        protected override string GetGuiErrorMessage() {
            if (Resolver == null) {
                return $"Not found {nameof(ModelViewerDebugObjectResolver)}";
            }

            return base.GetGuiErrorMessage();
        }

        /// <inheritdoc/>
        protected override void OnInactiveGuiInternal() {
            if (GUILayout.Button("Play Scene")) {
                PlayScene("model_viewer");
            }
        }
    }
}