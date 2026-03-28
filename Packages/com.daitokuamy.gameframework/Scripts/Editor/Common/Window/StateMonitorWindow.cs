using GameFramework.DebugSystem.Editor;
using UnityEditor;

namespace GameFramework.Editor {
    /// <summary>
    /// Stateの情報監視用ウィンドウ
    /// </summary>
    public sealed partial class StateMonitorWindow : DebugWindowBase<StateMonitorWindow> {
        /// <summary>
        /// 開く処理
        /// </summary>
        [MenuItem("Window/Game Framework/State Monitor")]
        public static void Open() {
            GetWindow<StateMonitorWindow>(ObjectNames.NicifyVariableName(nameof(StateMonitorWindow)));
        }

        /// <inheritdoc/>
        protected override void OnEnableInternal() {
            AddPanel(new ContainerPanel());
            AddPanel(new RouterPanel());
        }
    }
}
