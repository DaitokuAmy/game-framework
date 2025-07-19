using GameFramework.DebugSystems.Editor;
using UnityEditor;

namespace GameFramework.Editor {
    /// <summary>
    /// Situationの情報監視用ウィンドウ
    /// </summary>
    public partial class SituationMonitorWindow : DebugWindowBase<SituationMonitorWindow> {
        /// <summary>
        /// 開く処理
        /// </summary>
        [MenuItem("Window/Game Framework/Situation Monitor")]
        public static void Open() {
            GetWindow<SituationMonitorWindow>(ObjectNames.NicifyVariableName(nameof(SituationMonitorWindow)));
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void OnEnableInternal() {
            AddPanel(new ContainerPanel());
            AddPanel(new FlowPanel());
        }
    }
}