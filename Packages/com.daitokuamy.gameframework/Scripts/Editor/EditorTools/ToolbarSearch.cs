using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// ツールバー検索入力ヘルパー
    /// </summary>
    public sealed class ToolbarSearch {
        private readonly SearchField _searchField = new();
        private string _text = "";

        /// <summary>検索文字列</summary>
        public string Text {
            get => _text;
            set => _text = value ?? "";
        }

        /// <summary>大文字小文字を区別するか</summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// ツールバー描画
        /// </summary>
        public void OnToolbarGUI(string label = "Search", float labelWidth = 46.0f, bool showCaseToggle = true) {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(label, GUILayout.Width(labelWidth));
            _text = _searchField.OnToolbarGUI(_text);

            if (showCaseToggle) {
                CaseSensitive = GUILayout.Toggle(CaseSensitive, "Aa", EditorStyles.toolbarButton, GUILayout.Width(30.0f));
            }

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(42.0f))) {
                _text = "";
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 一致判定
        /// </summary>
        public bool IsMatch(string target) {
            if (string.IsNullOrEmpty(_text)) {
                return true;
            }

            var value = target ?? string.Empty;
            var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return value.IndexOf(_text, comparison) >= 0;
        }
    }
}
