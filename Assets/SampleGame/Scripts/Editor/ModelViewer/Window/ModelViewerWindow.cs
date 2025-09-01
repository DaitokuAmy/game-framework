using GameFramework.Core;
using GameFramework.DebugSystems.Editor;
using SampleGame.Lifecycle;
using UnityEditor;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// モデルビューア用のWindow
    /// </summary>
    public partial class ModelViewerWindow : DebugWindowBase<ModelViewerWindow> {
        /// <summary>Service取得用</summary>
        private IServiceResolver Resolver => ModelViewerDebugServiceResolver.Instance;
        
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
            if (Resolver == null)
            {
                return $"Not found {nameof(ModelViewerDebugServiceResolver)}";
            }

            return base.GetGuiErrorMessage();
        }

        /// <inheritdoc/>
        protected override void OnInactiveGuiInternal()
        {
            if (GUILayout.Button("Play Scene"))
            {
                PlayScene("model_viewer");
            }
        }
    }
}