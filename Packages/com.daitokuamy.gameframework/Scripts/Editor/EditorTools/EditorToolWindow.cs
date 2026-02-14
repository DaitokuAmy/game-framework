using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFramework.EditorTools.Editor {
    /// <summary>
    /// EditorToolModuleを統合管理するホストWindow
    /// </summary>
    public abstract class EditorToolWindow<TWindow, TConfigData, TUserData> : EditorWindow
        where TWindow : EditorToolWindow<TWindow, TConfigData, TUserData>
        where TConfigData : class, new()
        where TUserData : class, new() {
        [Serializable]
        private sealed class WindowConfigData {
            public TConfigData ConfigData = new();
        }

        [Serializable]
        private sealed class WindowEditorPrefsData {
            public int SelectedModuleIndex;
            public TUserData UserData = new();
        }

        /// <summary>タブ内左右余白</summary>
        private const float TabHorizontalPadding = 18.0f;
        /// <summary>タブ間の水平余白</summary>
        private const float TabHorizontalSpacing = 4.0f;
        /// <summary>タブ描画領域の左右マージン</summary>
        private const float TabAreaHorizontalMargin = 24.0f;

        private readonly List<EditorToolModule<TWindow, TConfigData, TUserData>> _modules = new();
        private readonly List<Vector2> _moduleScrollPositions = new();

        private int _selectedModuleIndex;
        private int _startedModuleIndex = -1;
        private TConfigData _config;
        private TUserData _user;
        private EditorPrefsState<WindowEditorPrefsData> _editorPrefsState;

        /// <summary>Config設定保存ファイルパス（必要ならoverride）</summary>
        protected virtual string ConfigDataFilePath => $"ProjectSettings/EditorToolWindowData/{GetType().Name}.json";
        /// <summary>EditorPrefsキー接頭辞</summary>
        protected virtual string EditorPrefsKeyPrefix => $"GameFramework.EditorTools.EditorToolWindow.{GetType().FullName}";
        /// <summary>有効状態か</summary>
        protected virtual bool IsActive => true;
        /// <summary>モジュール描画するか</summary>
        protected virtual bool IsDrawModule => true;

        /// <summary>Config設定データ</summary>
        protected TConfigData Config {
            get => _config;
            set => _config = value;
        }

        /// <summary>User設定データ</summary>
        protected TUserData User {
            get => _user;
            set => _user = value;
        }

        /// <summary>
        /// モジュール一覧を生成
        /// </summary>
        protected abstract IEnumerable<EditorToolModule<TWindow, TConfigData, TUserData>> CreateModules();

        /// <summary>エラーメッセージ取得</summary>
        protected virtual string GetGuiErrorMessage() => null;

        /// <summary>非アクティブ時描画</summary>
        protected virtual void OnInactiveGuiInternal() { }

        /// <summary>ヘッダー描画</summary>
        protected virtual void OnHeaderGuiInternal() { }

        /// <summary>フッター描画</summary>
        protected virtual void OnFooterGuiInternal() { }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _modules.Clear();
            _moduleScrollPositions.Clear();

            _config = LoadConfigDataFromFile();
            LoadEditorPrefsDataFromEditorPrefs();

            if (this is not TWindow typedWindow) {
                throw new InvalidOperationException($"{GetType().Name} must inherit EditorToolWindow<{GetType().Name}, {typeof(TConfigData).Name}, {typeof(TUserData).Name}>.");
            }

            var modules = CreateModules();
            _modules.AddRange(modules ?? Array.Empty<EditorToolModule<TWindow, TConfigData, TUserData>>());

            for (var i = 0; i < _modules.Count; i++) {
                _modules[i].Attach(typedWindow);
                _moduleScrollPositions.Add(Vector2.zero);
            }

            _selectedModuleIndex = Mathf.Clamp(_selectedModuleIndex, 0, Mathf.Max(0, _modules.Count - 1));

            SceneView.duringSceneGui += OnSceneGUIInternal;
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            SceneView.duringSceneGui -= OnSceneGUIInternal;
            SaveConfigDataToFile();
            SaveEditorPrefsDataToEditorPrefs();

            for (var i = 0; i < _modules.Count; i++) {
                _modules[i].Detach();
            }

            _modules.Clear();
            _moduleScrollPositions.Clear();
            _selectedModuleIndex = 0;
            _startedModuleIndex = -1;
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            if (!IsActive) {
                ExitCurrentModule();
                OnInactiveGuiInternal();
                return;
            }

            var errorMessage = GetGuiErrorMessage();
            if (!string.IsNullOrEmpty(errorMessage)) {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                return;
            }

            if (_modules.Count <= 0) {
                EditorGUILayout.HelpBox("No modules.", MessageType.Info);
                return;
            }

            EnsureCurrentModuleStarted();

            OnHeaderGuiInternal();
            DrawModuleTabs();
            if (IsDrawModule) {
                DrawSelectedModule();
            }

            OnFooterGuiInternal();
        }

        /// <summary>
        /// SceneGUI描画
        /// </summary>
        private void OnSceneGUIInternal(SceneView sceneView) {
            if (!IsActive || !IsDrawModule || _modules.Count <= 0) {
                return;
            }

            var module = _modules[_selectedModuleIndex];
            try {
                module.OnSceneGUI(sceneView);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// モジュールタブの描画
        /// </summary>
        private void DrawModuleTabs() {
            var prevSelected = _selectedModuleIndex;
            var style = EditorStyles.toolbarButton;
            var availableWidth = Mathf.Max(100.0f, EditorGUIUtility.currentViewWidth - TabAreaHorizontalMargin);
            var rows = BuildTabRows(style, availableWidth);

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++) {
                var row = rows[rowIndex];

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                for (var i = 0; i < row.Count; i++) {
                    var tabIndex = row[i];
                    var module = _modules[tabIndex];
                    var tabWidth = GetTabWidth(style, module.DisplayName, availableWidth);
                    var isSelected = tabIndex == _selectedModuleIndex;
                    if (GUILayout.Toggle(isSelected, module.DisplayName, style, GUILayout.Width(tabWidth))) {
                        _selectedModuleIndex = tabIndex;
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            _selectedModuleIndex = Mathf.Clamp(_selectedModuleIndex, 0, _modules.Count - 1);
            if (prevSelected != _selectedModuleIndex) {
                SaveEditorPrefsDataToEditorPrefs();
                SwitchStartedModule(_selectedModuleIndex);
            }
        }

        /// <summary>
        /// タブ行を生成
        /// </summary>
        private List<List<int>> BuildTabRows(GUIStyle style, float availableWidth) {
            var rows = new List<List<int>>();
            var currentRow = new List<int>();
            var currentWidth = 0.0f;

            for (var i = 0; i < _modules.Count; i++) {
                var tabWidth = GetTabWidth(style, _modules[i].DisplayName, availableWidth);
                var requiredWidth = currentRow.Count <= 0 ? tabWidth : currentWidth + TabHorizontalSpacing + tabWidth;

                if (currentRow.Count > 0 && requiredWidth > availableWidth) {
                    rows.Add(currentRow);
                    currentRow = new List<int>();
                    currentWidth = 0.0f;
                }

                if (currentRow.Count > 0) {
                    currentWidth += TabHorizontalSpacing;
                }

                currentRow.Add(i);
                currentWidth += tabWidth;
            }

            if (currentRow.Count > 0) {
                rows.Add(currentRow);
            }

            return rows;
        }

        /// <summary>
        /// タブ幅を取得
        /// </summary>
        private static float GetTabWidth(GUIStyle style, string label, float maxWidth) {
            var content = new GUIContent(label);
            var textWidth = style.CalcSize(content).x;
            return Mathf.Min(maxWidth, textWidth + TabHorizontalPadding);
        }

        /// <summary>
        /// 選択中モジュールの描画
        /// </summary>
        private void DrawSelectedModule() {
            var module = _modules[_selectedModuleIndex];
            var scrollPosition = _moduleScrollPositions[_selectedModuleIndex];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            try {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                try {
                    module.OnGUI();
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
                finally {
                    EditorGUILayout.EndScrollView();
                }
            }
            finally {
                EditorGUILayout.EndVertical();
            }

            _moduleScrollPositions[_selectedModuleIndex] = scrollPosition;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (!IsActive || _modules.Count <= 0) {
                return;
            }

            for (var i = 0; i < _modules.Count; i++) {
                _modules[i].OnEveryUpdate();
            }

            _modules[_selectedModuleIndex].OnUpdate();
            Repaint();
        }

        /// <summary>
        /// Config設定データを保存
        /// </summary>
        protected void SaveConfigData() {
            SaveConfigDataToFile();
        }

        /// <summary>
        /// EditorPrefs設定データを保存
        /// </summary>
        protected void SaveEditorPrefsData() {
            SaveEditorPrefsDataToEditorPrefs();
        }

        /// <summary>
        /// Config設定データを再読込
        /// </summary>
        protected void ReloadConfigData() {
            _config = LoadConfigDataFromFile();
        }

        /// <summary>
        /// EditorPrefs設定データを再読込
        /// </summary>
        protected void ReloadEditorPrefsData() {
            LoadEditorPrefsDataFromEditorPrefs();
            _selectedModuleIndex = Mathf.Clamp(_selectedModuleIndex, 0, Mathf.Max(0, _modules.Count - 1));
        }

        /// <summary>
        /// Config設定ファイルを読込
        /// </summary>
        private TConfigData LoadConfigDataFromFile() {
            var fullPath = GetConfigDataFileFullPath();
            if (!File.Exists(fullPath)) {
                return new TConfigData();
            }

            try {
                var json = File.ReadAllText(fullPath);
                if (string.IsNullOrEmpty(json)) {
                    return new TConfigData();
                }

                var data = JsonUtility.FromJson<WindowConfigData>(json);
                return data?.ConfigData ?? new TConfigData();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return new TConfigData();
            }
        }

        /// <summary>
        /// Config設定ファイルへ保存
        /// </summary>
        private void SaveConfigDataToFile() {
            var fullPath = GetConfigDataFileFullPath();

            try {
                var dirPath = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath)) {
                    Directory.CreateDirectory(dirPath);
                }

                var saveData = new WindowConfigData {
                    ConfigData = _config ?? new TConfigData()
                };
                var json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(fullPath, json);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Config設定ファイルの絶対パスを取得
        /// </summary>
        private string GetConfigDataFileFullPath() {
            if (Path.IsPathRooted(ConfigDataFilePath)) {
                return ConfigDataFilePath;
            }

            var projectRootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRootPath, ConfigDataFilePath));
        }

        /// <summary>
        /// EditorPrefs設定データを読込
        /// </summary>
        private void LoadEditorPrefsDataFromEditorPrefs() {
            EnsureEditorPrefsState();
            _editorPrefsState.Load();

            var value = _editorPrefsState.Value ?? new WindowEditorPrefsData();
            _selectedModuleIndex = value.SelectedModuleIndex;
            _user = value.UserData ?? new TUserData();
        }

        /// <summary>
        /// EditorPrefs設定データを保存
        /// </summary>
        private void SaveEditorPrefsDataToEditorPrefs() {
            EnsureEditorPrefsState();
            _editorPrefsState.Value.SelectedModuleIndex = _selectedModuleIndex;
            _editorPrefsState.Value.UserData = _user ?? new TUserData();
            _editorPrefsState.Save();
        }

        /// <summary>
        /// EditorPrefs状態を初期化
        /// </summary>
        private void EnsureEditorPrefsState() {
            if (_editorPrefsState != null) {
                return;
            }

            _editorPrefsState = new EditorPrefsState<WindowEditorPrefsData>(GetEditorPrefsStateKey());
            _editorPrefsState.Load();
        }

        /// <summary>
        /// EditorPrefs状態キーを取得
        /// </summary>
        private string GetEditorPrefsStateKey() {
            return $"{EditorPrefsKeyPrefix}.State";
        }

        /// <summary>
        /// カレントモジュール開始を保証
        /// </summary>
        private void EnsureCurrentModuleStarted() {
            if (_startedModuleIndex == _selectedModuleIndex) {
                return;
            }

            SwitchStartedModule(_selectedModuleIndex);
        }

        /// <summary>
        /// 開始中モジュールを切替
        /// </summary>
        private void SwitchStartedModule(int nextIndex) {
            ExitCurrentModule();
            StartModule(nextIndex);
        }

        /// <summary>
        /// モジュールを開始
        /// </summary>
        private void StartModule(int index) {
            if (index < 0 || index >= _modules.Count) {
                return;
            }

            _modules[index].Start();
            _startedModuleIndex = index;
        }

        /// <summary>
        /// カレントモジュールを終了
        /// </summary>
        private void ExitCurrentModule() {
            if (_startedModuleIndex < 0 || _startedModuleIndex >= _modules.Count) {
                _startedModuleIndex = -1;
                return;
            }

            _modules[_startedModuleIndex].Exit();
            _startedModuleIndex = -1;
        }
    }
}
