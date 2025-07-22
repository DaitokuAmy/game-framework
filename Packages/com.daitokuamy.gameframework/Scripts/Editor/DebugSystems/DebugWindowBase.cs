using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.DebugSystems.Editor {
    /// <summary>
    /// デバッグ用ウィンドウの基底
    /// </summary>
    public abstract class DebugWindowBase<TWindow> : EditorWindow
        where TWindow : DebugWindowBase<TWindow> {
        /// <summary>
        /// パネル基底
        /// </summary>
        public abstract class PanelBase : IDisposable {
            private class SearchInfo {
                public SearchField SearchField;
                public string FilterText;
            }

            private readonly Dictionary<string, bool> _foldouts = new();
            private readonly Dictionary<string, Vector2> _scrolls = new();
            private readonly Dictionary<string, SearchInfo> _searchInfos = new();

            private Vector2 _scroll = Vector2.zero;
            private DisposableScope _scope;
            private GUIStyle _centerLabel;

            /// <summary>表示ラベル</summary>
            public abstract string Label { get; }

            /// <summary>
            /// 開始処理
            /// </summary>
            public void Start(TWindow window) {
                if (_scope != null) {
                    return;
                }

                _scope = new DisposableScope();
                _centerLabel = new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.UpperCenter
                };
                StartInternal(window, _scope);
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            public void Exit(TWindow window) {
                if (_scope == null) {
                    return;
                }

                var scope = _scope;
                _scope = null;
                ExitInternal(window);
                scope.Dispose();
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (_scope == null) {
                    return;
                }

                DisposeInternal();
                _scope.Dispose();
                _scope = null;
            }

            /// <summary>
            /// Gui描画
            /// </summary>
            public void DrawGui(TWindow window) {
                DrawHeaderGuiInternal(window);

                using (var scope = new EditorGUILayout.ScrollViewScope(_scroll)) {
                    DrawGuiInternal(window);
                    _scroll = scope.scrollPosition;
                }

                DrawFooterGuiInternal(window);
            }

            /// <summary>
            /// SceneビューのGui描画
            /// </summary>
            public void DrawSceneGui(SceneView sceneView, TWindow window) {
                if (window == null) {
                    return;
                }

                DrawSceneGuiInternal(sceneView, window);
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public void Update(TWindow window) {
                if (window == null) {
                    return;
                }

                UpdateInternal(window);
            }

            /// <summary>
            /// 常時更新処理
            /// </summary>
            public void EveryUpdate(TWindow window) {
                if (window == null) {
                    return;
                }

                EveryUpdateInternal(window);
            }

            /// <summary>
            /// 開始処理
            /// </summary>
            protected virtual void StartInternal(TWindow window, IScope scope) {
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            protected virtual void ExitInternal(TWindow window) {
            }

            /// <summary>
            /// 廃棄処理
            /// </summary>
            protected virtual void DisposeInternal() {
            }

            /// <summary>
            /// HeaderGui描画
            /// </summary>
            protected virtual void DrawHeaderGuiInternal(TWindow window) {
            }

            /// <summary>
            /// Gui描画
            /// </summary>
            protected abstract void DrawGuiInternal(TWindow window);

            /// <summary>
            /// FooterGui描画
            /// </summary>
            protected virtual void DrawFooterGuiInternal(TWindow window) {
            }

            /// <summary>
            /// SceneビューのGui描画
            /// </summary>
            protected virtual void DrawSceneGuiInternal(SceneView sceneView, TWindow window) {
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            protected virtual void UpdateInternal(TWindow window) {
            }

            /// <summary>
            /// 常時更新処理
            /// </summary>
            protected virtual void EveryUpdateInternal(TWindow window) {
            }

            /// <summary>
            /// コンテンツの描画
            /// </summary>
            protected void DrawContent<T>(string title, T target, Action<T> onDraw) {
                using (new EditorGUILayout.VerticalScope("Box")) {
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    onDraw?.Invoke(target);
                    EditorGUI.indentLevel--;
                }
            }

            /// <summary>
            /// コンテンツの描画
            /// </summary>
            protected void DrawContent(string title, Action onDraw) {
                using (new EditorGUILayout.VerticalScope("Box")) {
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    onDraw?.Invoke();
                    EditorGUI.indentLevel--;
                }
            }

            /// <summary>
            /// コンテンツの描画
            /// </summary>
            protected void DrawFoldoutContent<T>(string title, string key, T target, Action<T> onDraw, bool defaultOpen = false) {
                if (!_foldouts.TryGetValue(key, out var foldout)) {
                    foldout = defaultOpen;
                    _foldouts[title] = foldout;
                }

                _foldouts[key] = EditorGUILayout.Foldout(foldout, title, EditorStyles.foldoutHeader);
                if (_foldouts[key]) {
                    using (new EditorGUILayout.VerticalScope("Box")) {
                        EditorGUI.indentLevel++;
                        onDraw?.Invoke(target);
                        EditorGUI.indentLevel--;
                    }
                }
            }

            /// <summary>
            /// コンテンツの描画
            /// </summary>
            protected void DrawFoldoutContent<T>(string title, T target, Action<T> onDraw, bool defaultOpen = false) {
                DrawFoldoutContent(title, title, target, onDraw, defaultOpen);
            }

            /// <summary>
            /// コンテンツの描画
            /// </summary>
            protected void DrawFoldoutContent(string title, string key, Action onDraw, bool defaultOpen = false) {
                if (!_foldouts.TryGetValue(key, out var foldout)) {
                    foldout = defaultOpen;
                    _foldouts[title] = foldout;
                }

                _foldouts[key] = EditorGUILayout.Foldout(foldout, title, EditorStyles.foldoutHeader);
                if (_foldouts[key]) {
                    using (new EditorGUILayout.VerticalScope("Box")) {
                        EditorGUI.indentLevel++;
                        onDraw?.Invoke();
                        EditorGUI.indentLevel--;
                    }
                }
            }

            /// <summary>
            /// コンテンツの描画
            /// </summary>
            protected void DrawFoldoutContent(string title, Action onDraw, bool defaultOpen = false) {
                DrawFoldoutContent(title, title, onDraw, defaultOpen);
            }

            /// <summary>
            /// スクロール付きコンテンツの描画
            /// </summary>
            protected void DrawScrollContent<T>(string title, string key, T target, float height, Action<T> onDraw) {
                if (!_scrolls.TryGetValue(key, out var scroll)) {
                    scroll = Vector2.zero;
                    _scrolls[key] = scroll;
                }

                using (var scope = new EditorGUILayout.ScrollViewScope(scroll, "Box", GUILayout.MaxHeight(height))) {
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    onDraw?.Invoke(target);
                    EditorGUI.indentLevel--;
                    _scrolls[key] = scope.scrollPosition;
                }
            }

            /// <summary>
            /// スクロール付きコンテンツの描画
            /// </summary>
            protected void DrawScrollContent<T>(string title, T target, float height, Action<T> onDraw) {
                DrawScrollContent(title, title, target, height, onDraw);
            }

            /// <summary>
            /// スクロール付きコンテンツの描画
            /// </summary>
            protected void DrawScrollContent(string title, string key, float height, Action onDraw) {
                if (!_scrolls.TryGetValue(key, out var scroll)) {
                    scroll = Vector2.zero;
                    _scrolls[key] = scroll;
                }

                using (var scope = new EditorGUILayout.ScrollViewScope(scroll, "Box", GUILayout.MaxHeight(height))) {
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    onDraw?.Invoke();
                    EditorGUI.indentLevel--;
                    _scrolls[key] = scope.scrollPosition;
                }
            }

            /// <summary>
            /// スクロール付きコンテンツの描画
            /// </summary>
            protected void DrawScrollContent(string title, float height, Action onDraw) {
                DrawScrollContent(title, title, height, onDraw);
            }

            /// <summary>
            /// フィルタ機能付きのPopup描画
            /// </summary>
            protected int DrawFilteredPopup(string label, int selectedIndex, IReadOnlyList<string> displayedOptions) {
                if (string.IsNullOrEmpty(label)) {
                    return selectedIndex;
                }

                if (!_searchInfos.TryGetValue(label, out var searchInfo)) {
                    searchInfo = new SearchInfo {
                        SearchField = new SearchField(),
                        FilterText = ""
                    };
                    _searchInfos[label] = searchInfo;
                }

                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField(label,
                        GUILayout.Width(EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15));
                    searchInfo.FilterText = searchInfo.SearchField.OnToolbarGUI(searchInfo.FilterText);
                }

                _searchInfos[label] = searchInfo;
                var splitTexts = searchInfo.FilterText.Split(" ");
                var filteredOptions = new List<string>();
                var indexList = new List<int>();
                for (var i = 0; i < displayedOptions.Count; i++) {
                    var item = displayedOptions[i];
                    if (i == selectedIndex) {
                        selectedIndex = filteredOptions.Count;
                    }

                    if (splitTexts.All(x => string.IsNullOrEmpty(x) || item.Contains(x))) {
                        filteredOptions.Add(item);
                        indexList.Add(i);
                    }
                }

                if (filteredOptions.Count <= 0) {
                    return selectedIndex;
                }

                var index = EditorGUILayout.Popup(" ", selectedIndex, filteredOptions.ToArray());
                return indexList[index];
            }

            /// <summary>
            /// 矢印による項目選択の描画
            /// </summary>
            protected int DrawSelectItem(int index, string title, IReadOnlyList<string> labels) {
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField(title);
                    return DrawSelectItem(index, labels);
                }
            }

            /// <summary>
            /// 矢印による項目選択の描画
            /// </summary>
            protected int DrawSelectItem(int index, IReadOnlyList<string> labels) {
                index = Mathf.Clamp(index, 0, labels.Count - 1);

                using (new EditorGUILayout.HorizontalScope()) {
                    var prevCurrentIndex = index;
                    GUI.enabled = index > 0;
                    if (GUILayout.Button("<", GUILayout.Width(50))) {
                        index = Mathf.Max(0, index - 1);
                    }

                    GUI.enabled = true;
                    EditorGUILayout.LabelField(labels[index], _centerLabel);

                    GUI.enabled = index < labels.Count - 1;
                    if (GUILayout.Button(">", GUILayout.Width(50))) {
                        index = Mathf.Min(labels.Count - 1, index + 1);
                    }

                    if (prevCurrentIndex != index) {
                        GUI.changed = true;
                    }

                    GUI.enabled = true;
                }

                return index;
            }

            /// <summary>
            /// 中央揃え
            /// </summary>
            protected void DrawCenteringForHorizontal(Action onDraw) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    onDraw?.Invoke();
                    GUILayout.FlexibleSpace();
                }
            }

            /// <summary>
            /// 仕切り線の描画
            /// </summary>
            protected void DrawSeparator(Color color, bool useIndentLevel = false) {
                using (new GUILayout.HorizontalScope()) {
                    var width = EditorGUIUtility.currentViewWidth - 100;
                    var rect = GUILayoutUtility.GetRect(width, width, 11.0f, 11.0f);
                    if (useIndentLevel) {
                        rect = EditorGUI.IndentedRect(rect);
                    }

                    rect.yMin += 5;
                    rect.yMax -= 5;
                    EditorGUI.DrawRect(rect, color);
                }
            }

            /// <summary>
            /// 仕切り線の描画
            /// </summary>
            protected void DrawSeparator(bool useIndentLevel = false) {
                DrawSeparator(Color.gray, useIndentLevel);
            }
        }

        private readonly List<PanelBase> _panels = new();

        private int _currentTabIndex;

        /// <summary>更新するか</summary>
        protected virtual bool IsActive => Application.isPlaying;

        /// <summary>パネルを描画するか</summary>
        protected virtual bool IsDrawPanel => true;

        /// <summary>パネルリスト</summary>
        protected IReadOnlyList<PanelBase> Panels => _panels;

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected virtual void OnEnableInternal() {
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected virtual void OnDisableInternal() {
        }

        /// <summary>
        /// エラーメッセージの表示
        /// </summary>
        protected virtual string GetGuiErrorMessage() {
            return null;
        }

        /// <summary>
        /// アクティブではない時のGUI描画
        /// </summary>
        protected virtual void OnInactiveGuiInternal() {
        }

        /// <summary>
        /// ヘッダー要素のGUI描画
        /// </summary>
        protected virtual void OnHeaderGuiInternal() {
        }

        /// <summary>
        /// フッター要素のGUI描画
        /// </summary>
        protected virtual void OnFooterGuiInternal() {
        }

        /// <summary>
        /// パネルの追加
        /// </summary>
        protected void AddPanel(PanelBase panel) {
            _panels.Add(panel);
        }

        /// <summary>
        /// シーンの再生(isPlaying:falseのみ)
        /// </summary>
        protected void PlayScene(string sceneName) {
            if (Application.isPlaying) {
                return;
            }

            var guids = AssetDatabase.FindAssets($"t:scene {sceneName}");
            if (guids.Length <= 0) {
                Debug.LogWarning($"Not found scene. name:{sceneName}");
                return;
            }

            var foundPath = "";
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != sceneName) {
                    continue;
                }

                foundPath = path;
                break;
            }

            if (string.IsNullOrEmpty(foundPath)) {
                return;
            }

            EditorSceneManager.OpenScene(foundPath);
            EditorApplication.EnterPlaymode();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;

            // パネルの作成
            _panels.Clear();
            OnEnableInternal();
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            OnDisableInternal();

            foreach (var panel in _panels) {
                panel.Dispose();
            }

            _panels.Clear();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            if (!IsActive) {
                foreach (var panel in _panels) {
                    panel.Exit((TWindow)this);
                }

                OnInactiveGuiInternal();
                return;
            }

            var errorMessage = GetGuiErrorMessage();
            if (!string.IsNullOrEmpty(errorMessage)) {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                return;
            }

            foreach (var panel in _panels) {
                panel.Start((TWindow)this);
            }

            // ヘッダー
            OnHeaderGuiInternal();

            // カレントなパネルGUIの描画
            var labels = _panels.Select(x => x.Label).ToArray();
            _currentTabIndex = GUILayout.Toolbar(_currentTabIndex, labels, EditorStyles.toolbarButton);

            if (_currentTabIndex >= 0) {
                if (IsDrawPanel) {
                    _panels[_currentTabIndex].DrawGui((TWindow)this);
                }
            }

            // フッター
            OnFooterGuiInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (!IsActive) {
                return;
            }

            // 常時更新
            for (var i = 0; i < _panels.Count; i++) {
                _panels[i].EveryUpdate((TWindow)this);
            }

            // カレントなパネルGUIの更新
            if (_currentTabIndex >= 0) {
                _panels[_currentTabIndex].Update((TWindow)this);
            }

            Repaint();
        }

        /// <summary>
        /// SceneビューのGUI描画
        /// </summary>
        private void OnSceneGUI(SceneView sceneView) {
            if (!IsActive) {
                return;
            }

            // カレントなパネルGUIの描画
            if (_currentTabIndex >= 0) {
                if (IsDrawPanel) {
                    _panels[_currentTabIndex].DrawSceneGui(sceneView, (TWindow)this);
                }
            }
        }
    }
}