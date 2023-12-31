﻿using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameFramework.Kinematics.Editor {
    /// <summary>
    /// Constraint用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(ConstraintExpression), true)]
    public class ConstraintExpressionEditor : UnityEditor.Editor {
        private ReorderableList _sourceList = null;
        private SerializedProperty _sourcesProperty = null;
        private SerializedProperty _settingsProperty = null;

        /// <summary>
        /// インスペクタGUI描画
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawGUI();
            serializedObject.ApplyModifiedProperties();
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
            // 設定描画
            EditorGUILayout.PropertyField(_settingsProperty, true);
            // プロパティ描画
            DrawProperties();

            // ターゲット情報描画
            _sourceList.DoLayoutList();
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