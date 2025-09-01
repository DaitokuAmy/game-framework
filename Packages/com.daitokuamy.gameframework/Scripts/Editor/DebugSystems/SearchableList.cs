using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GameFramework.DebugSystems.Editor {
    /// <summary>
    /// 検索可能なリストGUI
    /// </summary>
    public class SearchableList<T> {
        private readonly SearchField _searchField;
        private string _filter;
        private Vector2 _scroll;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SearchableList() {
            _searchField = new SearchField();
            _filter = "";
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        /// <param name="items">表示項目</param>
        /// <param name="itemToName">項目名変換関数</param>
        /// <param name="onGUIElement">項目GUI描画</param>
        /// <param name="options">LayoutOption</param>
        public void OnGUI(IReadOnlyList<T> items, Func<T, string> itemToName, Action<T, int> onGUIElement,
            params GUILayoutOption[] options) {
            using (new EditorGUILayout.VerticalScope("Box", options)) {
                // 検索フィルタ
                _filter = _searchField.OnToolbarGUI(_filter);
                var filteredItems = items
                    .Select((x, i) => (x, i))
                    .Where(pair => {
                        if (string.IsNullOrEmpty(_filter)) {
                            return true;
                        }

                        var splitFilters = _filter.Split(" ");
                        var name = itemToName.Invoke(pair.x);
                        foreach (var filter in splitFilters) {
                            if (!name.Contains(filter)) {
                                return false;
                            }
                        }

                        return true;
                    })
                    .ToArray();

                // 項目描画
                using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, "Box")) {
                    for (var i = 0; i < filteredItems.Length; i++) {
                        onGUIElement.Invoke(filteredItems[i].x, filteredItems[i].i);
                    }

                    _scroll = scope.scrollPosition;
                }
            }
        }
    }
}