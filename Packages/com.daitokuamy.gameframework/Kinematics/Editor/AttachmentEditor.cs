using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameFramework.Kinematics.Editor {
    /// <summary>
    /// Attachment用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(Attachment), true)]
    public class AttachmentEditor : UnityEditor.Editor {
        private ReorderableList _sourceList;
        private SerializedProperty _activeProp;
        private SerializedProperty _lockProp;
        private SerializedProperty _updateModeProp;
        private SerializedProperty _sourcesProp;
        private SerializedProperty _settingsProp;

        /// <summary>
        /// インスペクタGUI描画
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawGUI();

            // Transformが動いた際の座標転送
            var transform = (target as MonoBehaviour)?.transform;
            if (transform != null && transform.hasChanged && !_lockProp.boolValue) {
                TransferOffset();
                transform.hasChanged = false;
            }

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
            EditorUtility.SetDirty(attachment);
        }

        /// <summary>
        /// Offsetのゼロクリア
        /// </summary>
        protected virtual void ResetOffset() {
            var attachment = (Attachment)target;
            attachment.ResetOffset();
            attachment.ApplyTransform();
            EditorUtility.SetDirty(attachment);
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
            EditorGUILayout.PropertyField(_updateModeProp);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope()) {
                // 有効化
                if (GUILayout.Button("Activate")) {
                    _activeProp.boolValue = true;
                    _lockProp.boolValue = true;
                }

                // ゼロ
                if (GUILayout.Button("Zero")) {
                    ResetOffset();
                    _activeProp.boolValue = true;
                    _lockProp.boolValue = true;
                }
            }

            // アクティブ
            EditorGUILayout.PropertyField(_activeProp);

            // ロック
            using (new EditorGUI.DisabledScope(Application.isPlaying)) {
                EditorGUILayout.PropertyField(_lockProp);
            }

            using (new EditorGUI.DisabledScope(_lockProp.boolValue)) {
                // 設定描画
                EditorGUILayout.PropertyField(_settingsProp, true);
                // プロパティ描画
                DrawProperties();
            }

            // ターゲット情報描画
            using (var scope = new EditorGUI.ChangeCheckScope()) {
                _sourceList.DoLayoutList();
                if (scope.changed) {
                    TransferOffset();
                }
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
            var element = _sourcesProp.GetArrayElementAtIndex(index);
            var itr = element.Copy();
            var endProp = itr.GetEndProperty();
            var r = rect;

            itr.NextVisible(true);
            r.y += EditorGUIUtility.standardVerticalSpacing;
            while (!SerializedProperty.EqualContents(itr, endProp)) {
                var height = EditorGUI.GetPropertyHeight(itr, true) + EditorGUIUtility.standardVerticalSpacing;
                r.height = height;
                EditorGUI.PropertyField(r, itr, true);
                itr.NextVisible(false);
                r.y += r.height;
            }
        }

        /// <summary>
        /// リストの要素高さ取得コールバック
        /// </summary>
        private float OnElementHeight(int index) {
            var element = _sourcesProp.GetArrayElementAtIndex(index);
            var itr = element.Copy();
            var endProp = itr.GetEndProperty();
            var totalHeight = 0.0f;

            itr.NextVisible(true);
            while (!SerializedProperty.EqualContents(itr, endProp)) {
                totalHeight += EditorGUI.GetPropertyHeight(itr, true) + EditorGUIUtility.standardVerticalSpacing;
                itr.NextVisible(false);
            }

            return totalHeight;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _activeProp = serializedObject.FindProperty("_active");
            _lockProp = serializedObject.FindProperty("_lock");
            _updateModeProp = serializedObject.FindProperty("_updateMode");
            _sourcesProp = serializedObject.FindProperty("_sources");
            _settingsProp = serializedObject.FindProperty("_settings");

            _sourceList = new ReorderableList(serializedObject, _sourcesProp);
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