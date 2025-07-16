using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// ObjectSelectorAttribute型用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(ObjectSelectorAttribute))]
    public class ObjectSelectorPropertyDrawerPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.ObjectReference) {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var controlId = GUIUtility.GetControlID("ObjectSelector".GetHashCode(), FocusType.Keyboard, position);
            var objectFieldName = "ObjectField";
            var focused = GUI.GetNameOfFocusedControl() == objectFieldName;
            
            var labelRect = position;
            labelRect.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, label);
            
            var fieldRect = position;
            fieldRect.xMin = labelRect.xMax + EditorGUIUtility.standardVerticalSpacing;
            fieldRect.xMax -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            using (new EditorGUI.DisabledScope(false)) {
                var content = EditorGUIUtility.ObjectContent(property.objectReferenceValue, fieldInfo.FieldType);
                GUI.SetNextControlName(objectFieldName);
                if (GUI.Button(fieldRect, content, EditorStyles.objectField)) {
                    EditorGUIUtility.PingObject(property.objectReferenceValue);
                    GUI.FocusControl(objectFieldName);
                }
            }
            
            var buttonRect = position;
            buttonRect.xMin = buttonRect.xMax - EditorGUIUtility.singleLineHeight;
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("Pick"), EditorStyles.miniButtonMid)) {
                var attr = (ObjectSelectorAttribute)attribute;
                ObjectSelectorWindow.Show(fieldInfo.FieldType, attr.DefaultFilter, property.objectReferenceValue, x => {
                    property.serializedObject.Update();
                    property.objectReferenceValue = x;
                    property.serializedObject.ApplyModifiedProperties();
                }, attr.RootFolder);
            }
            
            if (focused) {
                var currentEvent = Event.current;
                // 選択中ならDeleteボタンで内容をクリアする
                if (currentEvent.type == EventType.KeyDown) {
                    if (currentEvent.keyCode == KeyCode.Delete || currentEvent.keyCode == KeyCode.Backspace) {
                        property.objectReferenceValue = null;
                        currentEvent.Use();
                    }
                }
            }
        }
    }
}