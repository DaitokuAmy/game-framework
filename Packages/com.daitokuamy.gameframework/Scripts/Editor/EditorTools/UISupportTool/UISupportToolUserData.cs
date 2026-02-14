using System;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UI作業を支援する統合ツールWindow
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// UISupportToolWindowのユーザー保存データ
        /// </summary>
        [Serializable]
        internal sealed class UISupportUserData {
            public bool EnableArrowNudge = true;
            public bool AutoRoundOnNudge = true;
            public float ArrowStep = 1.0f;
            public float ShiftArrowStep = 10.0f;

            public bool DrawAnchorVisualization = true;
            public bool DrawSafeAreaSimulation;
            public int SafeAreaPresetIndex;

            public float PreviewDuration = 0.35f;
            public float PreviewScale = 1.08f;
            public bool PreviewUseFade = true;
            public bool PreviewUseScale = true;

            public string RenameButtonPrefix = "Button_";
            public string RenameTextPrefix = "Text_";
            public string RenameImagePrefix = "Image_";

            public string PresenterNamespace = "GameFramework.UI";
            public string PresenterBaseClass = "MonoBehaviour";
        }
    }
}
