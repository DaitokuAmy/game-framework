using System;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UI作業を支援する統合ツールWindow
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// UISupportToolWindowの保存データ
        /// </summary>
        [Serializable]
        internal sealed class ConfigData {
            public string RenameButtonSuffix = "Button";
            public string RenameTextSuffix = "Text";
            public string RenameImageSuffix = "Image";
            public string TemplatePrefabFolderGuid = "";
        }
    }
}
