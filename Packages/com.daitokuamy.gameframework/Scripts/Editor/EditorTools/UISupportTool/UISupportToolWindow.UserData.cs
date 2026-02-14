using System;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UI作業を支援する統合ツールWindow
    /// </summary>
    internal sealed partial class UISupportToolWindow {
        /// <summary>
        /// UISupportToolWindowのユーザー別ローカル保存データ
        /// </summary>
        [Serializable]
        internal sealed class UserData {
            public bool EnableArrowNudge = true;
            public bool AutoRoundOnNudge = true;
            public float ArrowStep = 1.0f;
            public float ShiftArrowStep = 10.0f;
            public bool IsAutoUnpackTemplatePrefab;
        }
    }
}
