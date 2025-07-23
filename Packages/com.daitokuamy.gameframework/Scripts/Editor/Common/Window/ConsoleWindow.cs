using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.Editor {
    /// <summary>
    /// Unity標準Console風のログ表示ウィンドウ仮想描画、Collapse、フィルター、アイコンなどに対応
    /// </summary>
    public class ConsoleWindow : EditorWindow {
        /// <summary>
        /// ログエントリ1件分を表す構造体
        /// </summary>
        private class LogEntry {
            public string Condition;
            public string StackTrace;
            public LogType Type;
            public int Count = 1;
            public bool IsExpanded = false;

            public int Hash => (Condition + StackTrace + Type).GetHashCode();
        }

        private const float RowHeight = 20f;

        private static Texture2D s_logIcon;
        private static Texture2D s_warningIcon;
        private static Texture2D s_errorIcon;

        private readonly List<LogEntry> _entries = new();
        private readonly Dictionary<int, LogEntry> _collapsedEntries = new();
        private Vector2 _scroll;
        private LogEntry _selectedEntry;

        private bool _filterLog = true;
        private bool _filterWarning = true;
        private bool _filterError = true;
        private bool _collapse = false;
        private string _search = "";

        private int _maxEntries = 1000;

        /// <summary>
        /// メニューからウィンドウを開く
        /// </summary>
        [MenuItem("Window/Game Framework/Console")]
        private static void ShowWindow() {
            var window = GetWindow<ConsoleWindow>("Game Framework Console");
            window.minSize = new Vector2(400, 300);
        }

        /// <summary>
        /// ログ受信とアイコン読み込み
        /// </summary>
        private void OnEnable() {
            Application.logMessageReceived += OnLogReceived;

            s_logIcon ??= EditorGUIUtility.FindTexture("console.infoicon");
            s_warningIcon ??= EditorGUIUtility.FindTexture("console.warnicon");
            s_errorIcon ??= EditorGUIUtility.FindTexture("console.erroricon");
        }

        /// <summary>
        /// ログ受信解除
        /// </summary>
        private void OnDisable() {
            Application.logMessageReceived -= OnLogReceived;
        }

        /// <summary>
        /// ログ受信時に内部ログリストへ追加
        /// </summary>
        private void OnLogReceived(string condition, string stackTrace, LogType type) {
            var entry = new LogEntry {
                Condition = condition,
                StackTrace = stackTrace,
                Type = type
            };

            if (_collapse) {
                var hash = entry.Hash;
                if (_collapsedEntries.TryGetValue(hash, out var existing)) {
                    existing.Count++;
                    return;
                }

                _collapsedEntries[hash] = entry;
            }

            _entries.Add(entry);
            if (_entries.Count > _maxEntries)
                _entries.RemoveAt(0);

            Repaint();
        }

        /// <summary>
        /// ウィンドウ描画
        /// </summary>
        private void OnGUI() {
            DrawToolbar();
            DrawLogListVirtualized();
        }

        /// <summary>
        /// トップの検索バーおよびフィルタUI
        /// </summary>
        private void DrawToolbar() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            _filterLog = GUILayout.Toggle(_filterLog, new GUIContent(" Log", s_logIcon), EditorStyles.toolbarButton);
            _filterWarning = GUILayout.Toggle(_filterWarning, new GUIContent(" Warning", s_warningIcon), EditorStyles.toolbarButton);
            _filterError = GUILayout.Toggle(_filterError, new GUIContent(" Error", s_errorIcon), EditorStyles.toolbarButton);

            _collapse = GUILayout.Toggle(_collapse, "Collapse", EditorStyles.toolbarButton);

            GUILayout.Label("Max:", GUILayout.Width(35));
            _maxEntries = EditorGUILayout.IntField(_maxEntries, GUILayout.Width(60));

            GUILayout.FlexibleSpace();
            _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.Width(200));

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// スクロール＋仮想描画によりログをリスト表示。
        /// </summary>
        private void DrawLogListVirtualized() {
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(position.height * 0.6f));

            var source = _collapse ? _collapsedEntries.Values.ToList() : _entries;
            source.Reverse();
            var visible = source
                .Where(IsVisible)
                .ToList();

            var totalCount = visible.Count;
            var totalHeight = totalCount * RowHeight;

            var dummyRect = GUILayoutUtility.GetRect(0, totalHeight);
            var viewTop = _scroll.y;
            var viewBottom = _scroll.y + position.height * 0.6f;

            var first = Mathf.FloorToInt(viewTop / RowHeight);
            var last = Mathf.CeilToInt(viewBottom / RowHeight);
            first = Mathf.Clamp(first, 0, totalCount);
            last = Mathf.Clamp(last, 0, totalCount);

            // Space: 前方スキップ
            GUILayout.Space(first * RowHeight);

            for (var i = first; i < last; i++) {
                DrawLogRow(visible[i]);
            }

            // Space: 後方スキップ
            GUILayout.Space((totalCount - last) * RowHeight);

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// ログ1行の描画。クリックで選択。
        /// </summary>
        private void DrawLogRow(LogEntry entry) {
            var icon = GetIcon(entry.Type);
            var color = GetColor(entry.Type);
            var label = entry.Condition.Split('\n')[0];
            if (_collapse && entry.Count > 1)
                label += $" (x{entry.Count})";

            var style = new GUIStyle(EditorStyles.label) {
                normal = { textColor = color },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(2, 2, 2, 2)
            };

            var isSelected = entry == _selectedEntry;
            var bgColor = isSelected ? new Color(0.3f, 0.4f, 0.6f, 0.3f) : GUI.backgroundColor;

            var rect = GUILayoutUtility.GetRect(new GUIContent(" " + label, icon), style);
            EditorGUI.DrawRect(rect, bgColor);

            if (GUI.Button(rect, new GUIContent(" " + label, icon), style)) {
                _selectedEntry = entry;
            }
        }

        /// <summary>
        /// 選択中ログの詳細（StackTraceなど）を表示。
        /// </summary>
        private void DrawSelectedEntryDetail() {
            if (_selectedEntry == null) return;

            EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_selectedEntry.Condition + "\n" + _selectedEntry.StackTrace, MessageType.None);
        }

        /// <summary>
        /// ログが現在のフィルター条件に一致しているかを判定
        /// </summary>
        private bool IsVisible(LogEntry entry) {
            if (!_filterLog && entry.Type == LogType.Log) return false;
            if (!_filterWarning && entry.Type == LogType.Warning) return false;
            if (!_filterError && (entry.Type == LogType.Error || entry.Type == LogType.Exception || entry.Type == LogType.Assert)) return false;

            if (!string.IsNullOrEmpty(_search)) {
                var lower = _search.ToLowerInvariant();
                if (!entry.Condition.ToLowerInvariant().Contains(lower) &&
                    !entry.StackTrace.ToLowerInvariant().Contains(lower)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ログタイプに応じた色を返す
        /// </summary>
        private Color GetColor(LogType type) => type switch {
            LogType.Warning => new Color(1f, 0.65f, 0f),
            LogType.Error => Color.red,
            LogType.Exception => Color.magenta,
            LogType.Assert => Color.cyan,
            _ => Color.white
        };

        /// <summary>
        /// ログタイプに応じたアイコンを返す
        /// </summary>
        private Texture2D GetIcon(LogType type) => type switch {
            LogType.Warning => s_warningIcon,
            LogType.Error or LogType.Exception or LogType.Assert => s_errorIcon,
            _ => s_logIcon
        };
    }
}