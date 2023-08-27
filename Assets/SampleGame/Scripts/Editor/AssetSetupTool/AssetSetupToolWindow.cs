using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SampleGame.Editor {
    /// <summary>
    /// Asset初期化用ツールウィンドウ
    /// </summary>
    public partial class AssetSetupToolWindow : EditorWindow {
        /// <summary>
        /// パネルのタイプ
        /// </summary>
        private enum PanelType {
            Model,
            Motion,
            UI,
        }
        
        /// <summary>
        /// パネル描画用の基底クラス
        /// </summary>
        private abstract class Panel : IDisposable {
            private Vector2 _scroll;
        
            protected AssetSetupToolWindow Window { get; }
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Panel(AssetSetupToolWindow window) {
                Window = window;
            }
        
            /// <summary>
            /// 初期化
            /// </summary>
            public void Initialize() {
                InitializeInternal();
            }

            /// <summary>
            /// 廃棄処理
            /// </summary>
            public void Dispose() {
                DisposeInternal();
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

            protected virtual void InitializeInternal() {}
            protected virtual void DisposeInternal() {}
            protected abstract void OnGUIInternal();
        }

        // 現在のパネルタイプ
        private PanelType _currentPanelType = PanelType.Model;
        // 各種パネル描画用インスタンス
        private Dictionary<PanelType, Panel> _panels = new();

        /// <summary>
        /// ウィンドウを開く
        /// </summary>
        [MenuItem("Window/Sample Game/Asset Setup Tool")]
        public static void Open() {
            GetWindow<AssetSetupToolWindow>(ObjectNames.NicifyVariableName(nameof(AssetSetupToolWindow)));
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            foreach (var pair in _panels) {
                pair.Value.Dispose();
            }
            _panels.Clear();
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            var toolbarLabels = Enum.GetNames(typeof(PanelType));
            _currentPanelType = (PanelType)GUILayout.Toolbar((int)_currentPanelType, toolbarLabels, EditorStyles.toolbarButton);
            var panel = GetPanel(_currentPanelType);
            if (panel != null) {
                panel.OnGUI();
            }
        }

        /// <summary>
        /// Panelの取得
        /// </summary>
        private Panel GetPanel(PanelType type) {
            if (_panels.TryGetValue(type, out var panel)) {
                return panel;
            }

            switch (type) {
                case PanelType.Model:
                    panel = new ModelPanel(this);
                    break;
                case PanelType.Motion:
                    panel = new MotionPanel(this);
                    break;
            }

            if (panel != null) {
                panel.Initialize();
                _panels[type] = panel;
            }

            return panel;
        }
    }
}