using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UI作業を支援する統合ツールWindow
    /// </summary>
    internal sealed partial class UISupportToolWindow : EditorToolWindow<UISupportToolWindow, UISupportToolWindow.ConfigData, UISupportToolWindow.UserData> {
        /// <summary>
        /// Windowを開く
        /// </summary>
        [MenuItem("Window/Game Framework/UI Support Tool")]
        private static void Open() {
            var window = GetWindow<UISupportToolWindow>();
            window.titleContent = new GUIContent("UI Support");
            window.minSize = new Vector2(540.0f, 360.0f);
            window.Show();
        }

        /// <summary>
        /// モジュール一覧を生成
        /// </summary>
        protected override IEnumerable<EditorToolModule<UISupportToolWindow, ConfigData, UserData>> CreateModules() {
            yield return new TransformModule();
            yield return new BindingModule();
            yield return new DoctorModule();
            yield return new AutomationModule();
        }

        /// <summary>
        /// 設定データを保存
        /// </summary>
        private void SaveState() {
            SaveConfigData();
            SaveEditorPrefsData();
        }
    }
}
