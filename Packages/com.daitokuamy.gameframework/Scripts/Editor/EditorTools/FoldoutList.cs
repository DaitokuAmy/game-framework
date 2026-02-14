using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// FoldoutされたリストGUI
    /// </summary>
    public sealed class FoldoutList<T> {
        private readonly string _label;
        private bool _open;
        private Vector2 _scroll = Vector2.zero;

        /// <summary>展開状態</summary>
        public bool Open {
            get => _open;
            set => _open = value;
        }

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
        public void OnGUI(IReadOnlyList<T> items, Action<T, int> onGUIElement, params GUILayoutOption[] options) {
            _open = EditorGUILayout.ToggleLeft(_label, _open, EditorStyles.boldLabel);
            if (!_open) {
                return;
            }

            using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, "Box", options)) {
                for (var i = 0; i < items.Count; i++) {
                    onGUIElement?.Invoke(items[i], i);
                }

                _scroll = scope.scrollPosition;
            }
        }
    }
}
