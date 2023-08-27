using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SampleGame.VfxViewer.Editor {
    /// <summary>
    /// モデルビューア用のWindow
    /// </summary>
    public partial class VfxViewerWindow : EditorWindow {
        /// <summary>
        /// 検索可能なリストGUI
        /// </summary>
        private class SearchableList<T> {
            private SearchField _searchField;
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
            public void OnGUI(IList<T> items, Func<T, string> itemToName, Action<T, int> onGUIElement, params GUILayoutOption[] options) {
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

        /// <summary>
        /// 検索可能なリストGUI
        /// </summary>
        private class FoldoutList<T> {
            private readonly string _label;
            private bool _open;
            private Vector2 _scroll;
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public FoldoutList(string label, bool defaultOpen = true) {
                _label = label;
                _open = defaultOpen;
            }
            
            /// <summary>
            /// GUI描画
            /// </summary>
            /// <param name="items">表示項目</param>
            /// <param name="onGUIElement">項目GUI描画</param>
            /// <param name="options">LayoutOption</param>
            public void OnGUI(IList<T> items, Action<T, int> onGUIElement, params GUILayoutOption[] options) {
                _open = EditorGUILayout.ToggleLeft(_label, _open, EditorStyles.boldLabel);
                if (_open) {
                    // 項目描画
                    using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, "Box", options)) {
                        for (var i = 0; i < items.Count; i++) {
                            onGUIElement.Invoke(items[i], i);
                        }

                        _scroll = scope.scrollPosition;
                    }
                }
            }
        }

        /// <summary>
        /// タブ毎に描画する内容の規定
        /// </summary>
        private abstract class PanelBase : IDisposable {
            private DisposableScope _scope;
            private Vector2 _scroll;

            /// <summary>タブの表示名</summary>
            public abstract string Title { get; }
            /// <summary>Windowへの参照</summary>
            protected VfxViewerWindow Window { get; private set; }

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Initialize(VfxViewerWindow window) {
                _scope = new();
                Window = window;
                InitializeInternal(_scope);
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                DisposeInternal();
                _scope.Dispose();
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            public void OnGUI() {
                using (var scope = new EditorGUILayout.ScrollViewScope(_scroll)) {
                    OnGUIInternal();
                    _scroll = scope.scrollPosition;
                }
            }

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected virtual void InitializeInternal(IScope scope) {
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            protected virtual void DisposeInternal() {
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected abstract void OnGUIInternal();
        }

        private Dictionary<string, PanelBase> _panels = new();
        private string _currentPanelKey;

        /// <summary>
        /// 開く処理
        /// </summary>
        [MenuItem("Window/Sample Game/Vfx Viewer")]
        private static void Open() {
            GetWindow<VfxViewerWindow>("Vfx Viewer");
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            ClearPanels();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            Repaint();
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("Playing Mode Only", MessageType.Error);
                ClearPanels();

                if (GUILayout.Button("Play Scene")) {
                    PlayScene();
                }
                return;
            }
            
            var modelViewerModel = VfxViewerModel.Get();
            if (modelViewerModel == null) {
                EditorGUILayout.HelpBox("Not found VfxViewerModel", MessageType.Error);
                ClearPanels();
                return;
            }

            CreatePanels();

            var labels = _panels.Keys.ToArray();
            var index = labels.ToList().IndexOf(_currentPanelKey);
            index = GUILayout.Toolbar(index, labels, EditorStyles.toolbarButton);
            _currentPanelKey = index >= 0 ? labels[index] : "";
            var panel = GetPanel(_currentPanelKey);
            if (panel != null) {
                panel.OnGUI();
            }
        }

        /// <summary>
        /// パネルの取得
        /// </summary>
        private PanelBase GetPanel(string panelKey) {
            _panels.TryGetValue(panelKey, out var panel);
            return panel;
        }

        /// <summary>
        /// パネルの生成
        /// </summary>
        private void CreatePanels() {
            // パネルの生成
            void CreatePanel<T>()
                where T : PanelBase, new() {
                var panel = new T();
                
                if (_panels.ContainsKey(panel.Title)) {
                    return;
                }
                
                panel.Initialize(this);
                _panels[panel.Title] = panel;
            }
            
            CreatePanel<VfxPanel>();
            CreatePanel<EnvironmentPanel>();
            CreatePanel<RecordingPanel>();
            CreatePanel<SettingsPanel>();
        }

        /// <summary>
        /// パネルのクリア
        /// </summary>
        private void ClearPanels() {
            foreach (var pair in _panels) {
                pair.Value.Dispose();
            }

            _panels.Clear();
        }

        /// <summary>
        /// VfxViewerシーンの再生
        /// </summary>
        private void PlayScene() {
            if (Application.isPlaying) {
                return;
            }
            
            var guids = AssetDatabase.FindAssets("t:scene vfx_viewer");
            if (guids.Length <= 0) {
                Debug.LogWarning("Not found vfx_viewer scene.");
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            EditorSceneManager.OpenScene(path);
            EditorApplication.EnterPlaymode();
        }
    }
}