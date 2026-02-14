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

            public float PreviewDuration = 0.35f;
            public float PreviewScale = 1.08f;
            public bool PreviewUseFade = true;
            public bool PreviewUseScale = true;

            public string RenameButtonSuffix = "Button";
            public string RenameTextSuffix = "Text";
            public string RenameImageSuffix = "Image";
            public string TemplatePrefabFolderGuid = "";

        }
    }
}
