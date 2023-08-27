using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameFramework.Kinematics.Editor {
    /// <summary>
    /// Attachment用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(Attachment), true)]
    public class AttachmentEditor : UnityEditor.Editor {
        private ReorderableList _sourceList = null;
        private SerializedProperty _activeProperty = null;
        private SerializedProperty _updateModeProperty = null;
        private SerializedProperty _sourcesProperty = null;
        private SerializedProperty _settingsProperty = null;

        private bool _lock = false;

        /// <summary>
        /// インスペクタGUI描画
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawGUI();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Transform情報のOffset転送
        /// </summary>
        protected virtual void TransferOffset() {
            var attachment = (Attachment)target;
            // 現在のTransformで設定を更新
            attachment.TransferOffset();
            attachment.ApplyTransform();
            serializedObject.Update();
        }

        /// <summary>
        /// Offsetのゼロクリア
        /// </summary>
        protected virtual void ResetOffset() {
            var attachment = (Attachment)target;
            attachment.ResetOffset();
            attachment.ApplyTransform();
            serializedObject.Update();
        }

        /// <summary>
        /// 設定項目の描画
        /// </summary>
        protected virtual void DrawProperties() {
        }

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
        /// GUI描画
        /// </summary>
        private void DrawGUI() {
            // 更新モード
            EditorGUILayout.PropertyField(_updateModeProperty);

            EditorGUILayout.Space();

            // 有効化ボタン
            var prevColor = GUI.color;
            GUI.color = _activeProperty.boolValue ? Color.cyan : prevColor;

            if (GUILayout.Button(_activeProperty.boolValue ? " Active " : "Activate")) {
                _activeProperty.boolValue = !_activeProperty.boolValue;

                if (_activeProperty.boolValue) {
                    // アクティブ時にロックする
                    _lock = true;
                }
            }

            GUI.color = prevColor;

            using (new EditorGUILayout.HorizontalScope()) {
                // 転送ボタン
                using (new EditorGUI.DisabledScope(_lock)) {
                    if (GUILayout.Button("Transfer")) {
                        if (!_lock) {
                            TransferOffset();
                        }
                    }
                }

                // リセットボタン
                using (new EditorGUI.DisabledScope(_lock)) {
                    if (GUILayout.Button("Zero")) {
                        ResetOffset();
                    }
                }
            }

            // ロック
            _lock = EditorGUILayout.ToggleLeft("Lock", _lock);

            using (new EditorGUI.DisabledScope(_lock)) {
                // 設定描画
                EditorGUILayout.PropertyField(_settingsProperty, true);
                // プロパティ描画
                DrawProperties();

                // ターゲット情報描画
                _sourceList.DoLayoutList();
            }
        }

        /// <summary>
        /// リストの要素描画コールバック
        /// </summary>
        private void OnDrawHeader(Rect rect) {
            EditorGUI.LabelField(rect, "Source List", EditorStyles.boldLabel);
        }

        /// <summary>
        /// リストの要素描画コールバック
        /// </summary>
        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused) {
            var element = _sourcesProperty.GetArrayElementAtIndex(index);
            var elementName = element.displayName;
            var targetObj = element.FindPropertyRelative("target").objectReferenceValue;
            if (targetObj != null) {
                elementName = $"{index}:{targetObj.name}";
            }

            EditorGUI.PropertyField(rect, element, new GUIContent(elementName), true);
        }

        /// <summary>
        /// リストの要素高さ取得コールバック
        /// </summary>
        private float OnElementHeight(int index) {
            var element = _sourcesProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _activeProperty = serializedObject.FindProperty("_active");
            _updateModeProperty = serializedObject.FindProperty("_updateMode");
            _sourcesProperty = serializedObject.FindProperty("_sources");
            _settingsProperty = serializedObject.FindProperty("_settings");

            _sourceList = new ReorderableList(serializedObject, _sourcesProperty);
            _sourceList.drawHeaderCallback = OnDrawHeader;
            _sourceList.drawElementCallback = OnDrawElement;
            _sourceList.elementHeightCallback = OnElementHeight;

            OnEnableInternal();
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            if (_sourceList != null) {
                _sourceList.drawHeaderCallback = null;
                _sourceList.drawElementCallback = null;
                _sourceList.elementHeightCallback = null;
            }

            OnDisableInternal();
        }
    }
}