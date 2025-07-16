using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Editor {
    /// <summary>
    /// Object選択用のモーダルウィンドウ（表示中の項目のみ遅延ロード対応）
    /// </summary>
    public class ObjectSelectorWindow : EditorWindow {
        private const float EntryHeight = 18.0f * 2;

        private string _defaultFilter = "";
        private Object _currentObject;
        private Action<Object> _onSelected;
        private string _searchText = "";
        private Vector2 _scrollPosition;
        private string _rootFolder = "Assets";
        private List<string> _resultGuids = new();

        /// <summary>
        /// ウィンドウを表示する（フィルター文字列指定）
        /// </summary>
        public static void Show(string defaultFilter, Object currentObject, Action<Object> onSelected, string rootFolder = "Assets") {
            var window = CreateInstance<ObjectSelectorWindow>();
            window._defaultFilter = defaultFilter;
            window._currentObject = currentObject;
            window._onSelected = onSelected;
            window._rootFolder = rootFolder;
            window.titleContent = new GUIContent("Object Selector");
            window.minSize = new Vector2(100, 100);
            window.ShowUtility();
            window.Focus();
            window.RefreshFilteredObjects(true);
        }

        /// <summary>
        /// ウィンドウを表示する（フィルターなし）
        /// </summary>
        public static void Show(Object currentObject, Action<Object> onSelected, string rootFolder = "Assets") {
            Show("", currentObject, onSelected, rootFolder);
        }

        /// <summary>
        /// ウィンドウを表示する（型フィルター指定）
        /// </summary>
        public static void Show(Type typeFilter, Object currentObject, Action<Object> onSelected, string rootFolder = "Assets") {
            Show($"t:{typeFilter.Name}", currentObject, onSelected, rootFolder);
        }

        /// <summary>
        /// ウィンドウを表示する（型フィルター指定）
        /// </summary>
        public static void Show(Type typeFilter, string filter, Object currentObject, Action<Object> onSelected, string rootFolder = "Assets") {
            Show($"t:{typeFilter.Name} {filter}", currentObject, onSelected, rootFolder);
        }

        /// <summary>
        /// メインGUIの描画
        /// </summary>
        private void OnGUI() {
            DrawSearchBar();
            EditorGUILayout.Space(4);
            DrawScrollList();
            DrawFooter();
        }

        /// <summary>
        /// 検索バーを描画
        /// </summary>
        private void DrawSearchBar() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            _searchText = GUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck()) {
                RefreshFilteredObjects();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 遅延ロード付きのスクロールリスト描画
        /// </summary>
        private void DrawScrollList() {
            var total = _resultGuids.Count;
            var visibleCount = Mathf.CeilToInt(position.height / EntryHeight);
            var scrollY = _scrollPosition.y;
            var startIndex = Mathf.FloorToInt(scrollY / EntryHeight);
            var endIndex = Mathf.Min(startIndex + visibleCount + 5, total);

            using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition)) {
                GUILayout.Space(startIndex * EntryHeight);

                var currentGuid = _currentObject != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_currentObject)) : string.Empty;
                for (var i = startIndex; i < endIndex; i++) {
                    var guid = _resultGuids[i];
                    DrawObjectEntry(i, currentGuid == guid);
                }

                GUILayout.Space((total - endIndex) * EntryHeight);

                if (!_scrollPosition.Equals(scope.scrollPosition)) {
                    _scrollPosition = scope.scrollPosition;
                    Repaint();
                }
            }
        }

        /// <summary>
        /// フッターを描画
        /// </summary>
        private void DrawFooter() {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
                EditorGUILayout.Space(0.0f, true);
                if (GUILayout.Button("Clear")) {
                    _onSelected?.Invoke(null);
                }
            }
        }

        /// <summary>
        /// 単一オブジェクトエントリの描画と操作
        /// </summary>
        private void DrawObjectEntry(int index, bool current) {
            var guid = _resultGuids[index];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj == null) return;

            var prevColor = GUI.color;
            GUI.color = current ? Color.cyan : prevColor;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                var height = EditorGUIUtility.singleLineHeight;

                using (new EditorGUILayout.VerticalScope()) {
                    var content = EditorGUIUtility.ObjectContent(obj, typeof(Object));
                    content.text = obj.name;

                    if (GUILayout.Button(content, EditorStyles.objectField, GUILayout.MinWidth(400), GUILayout.ExpandWidth(true), GUILayout.Height(height))) {
                        _onSelected?.Invoke(obj);
                        Close();
                    }

                    GUILayout.Label(path, EditorStyles.miniLabel, GUILayout.MinWidth(400));
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Pick"), EditorStyles.miniButtonMid, GUILayout.Width(height), GUILayout.Height(height))) {
                    EditorGUIUtility.PingObject(obj);
                }
            }

            GUI.color = prevColor;
        }

        /// <summary>
        /// 検索結果をGUIDとして再取得
        /// </summary>
        private void RefreshFilteredObjects(bool focusScroll = false) {
            var guids = AssetDatabase.FindAssets($"{_defaultFilter} {_searchText}", new[] { _rootFolder });
            _resultGuids = guids.ToList();

            // カレントオブジェクトにフォーカスする
            if (focusScroll && _currentObject != null) {
                var currentGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_currentObject));
                var index = _resultGuids.IndexOf(currentGuid);
                if (index >= 0) {
                    _scrollPosition.y = index * EntryHeight;
                }
            }
        }
    }
}