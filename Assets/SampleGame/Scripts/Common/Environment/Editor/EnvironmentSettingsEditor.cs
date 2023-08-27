using GameFramework.Core;
using GameFramework.EnvironmentSystems;
using UnityEditor;
using UnityEngine;
using SampleGame;

namespace SampleGame.Editor {
    /// <summary>
    /// EnvironmentSettings用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(EnvironmentSettings))]
    public class EnvironmentSettingsEditor : UnityEditor.Editor {
        private SerializedProperty _data;
        private SerializedProperty _sun;
        private UnityEditor.Editor _dataEditor;

        /// <summary>
        /// インスペクタ描画
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var changed = GUI.changed || Event.current.commandName == "UndoRedoPerformed";

            if (changed) {
                ApplyDataEditor();
                ApplyEnvironment();
            }

            if (_dataEditor != null) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Context Data", EditorStyles.boldLabel);
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    _dataEditor.OnInspectorGUI();
                    if (scope.changed) {
                        ApplyEnvironment();
                    }
                }
            }
        }

        /// <summary>
        /// DataEditorの反映
        /// </summary>
        private void ApplyDataEditor() {
            var current = _data.objectReferenceValue;

            if (_dataEditor != null && _dataEditor.target == current) {
                return;
            }

            if (_dataEditor != null) {
                DestroyImmediate(_dataEditor);
                _dataEditor = null;
            }

            if (current != null) {
                _dataEditor = CreateEditor(current);
            }
        }

        /// <summary>
        /// 環境の適用
        /// </summary>
        private void ApplyEnvironment() {
            var settings = target as EnvironmentSettings;
            if (settings != null) {
                settings.ApplyEnvironment();
            }
        }

        /// <summary>
        /// アクティブ時
        /// </summary>
        private void OnEnable() {
            _data = serializedObject.FindProperty(nameof(_data));
            _sun = serializedObject.FindProperty(nameof(_sun));
            ApplyDataEditor();
        }

        /// <summary>
        /// 非アクティブ時
        /// </summary>
        private void OnDisable() {
            if (_dataEditor != null) {
                DestroyImmediate(_dataEditor);
                _dataEditor = null;
            }
        }
    }
}