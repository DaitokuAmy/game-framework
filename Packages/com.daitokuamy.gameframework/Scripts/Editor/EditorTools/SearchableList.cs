using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// 検索可能なリストGUI
    /// </summary>
    public sealed class SearchableList<T> {
        private readonly SearchField _searchField = new();
        private string _filter = "";
        private Vector2 _scroll = Vector2.zero;

        /// <summary>検索文字列</summary>
        public string Filter {
            get => _filter;
            set => _filter = value ?? "";
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        /// <param name="items">表示項目</param>
        /// <param name="itemToName">項目名変換関数</param>
        /// <param name="onGUIElement">項目GUI描画</param>
        /// <param name="options">LayoutOption</param>
        public void OnGUI(IReadOnlyList<T> items, Func<T, string> itemToName, Action<T, int> onGUIElement, params GUILayoutOption[] options) {
            using (new EditorGUILayout.VerticalScope("Box", options)) {
                _filter = _searchField.OnToolbarGUI(_filter);

                var filters = _filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var filteredItems = items
                    .Select((item, index) => (item, index))
                    .Where(x => CheckMatch(itemToName?.Invoke(x.item) ?? string.Empty, filters))
                    .ToArray();

                using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, "Box")) {
                    for (var i = 0; i < filteredItems.Length; i++) {
                        onGUIElement?.Invoke(filteredItems[i].item, filteredItems[i].index);
                    }

                    _scroll = scope.scrollPosition;
                }
            }
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        /// <param name="items">表示項目</param>
        /// <param name="onGUIElement">項目GUI描画</param>
        /// <param name="options">LayoutOption</param>
        public void OnGUI(IReadOnlyList<T> items, Action<T, int> onGUIElement, params GUILayoutOption[] options) {
            OnGUI(items, x => x?.ToString() ?? string.Empty, onGUIElement, options);
        }

        /// <summary>
        /// 検索一致判定
        /// </summary>
        private static bool CheckMatch(string name, IReadOnlyList<string> filters) {
            for (var i = 0; i < filters.Count; i++) {
                if (name.IndexOf(filters[i], StringComparison.OrdinalIgnoreCase) < 0) {
                    return false;
                }
            }

            return true;
        }
    }
}
