using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// UI作業を支援する統合ツールWindow
    /// </summary>
    internal sealed partial class UISupportToolWindow : EditorToolWindow<UISupportToolWindow, UISupportToolWindow.UISupportUserData> {
        /// <summary>Windowユーザー設定データ</summary>
        private UISupportUserData Data => UserData;

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
        protected override IEnumerable<EditorToolModule<UISupportToolWindow, UISupportUserData>> CreateModules() {
            yield return new TransformModule();
            yield return new BindingModule();
            yield return new DoctorModule();
            yield return new AutomationModule();
            yield return new PreviewModule();
        }

        /// <summary>
        /// ユーザーデータを保存
        /// </summary>
        private void SaveState() {
            SaveUserData();
        }
    }
}

