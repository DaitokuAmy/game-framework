using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// NullableEnum型用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(NullableEnum<>))]
    public class NullableEnumPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var type = fieldInfo.FieldType;
            var enumType = type.GetGenericArguments()[0];
            if (!enumType.IsEnum) {
                EditorGUI.LabelField(position, label, "T must be enum.");
                return;
            }
            
            var hasValueProp = property.FindPropertyRelative("_hasValue");
            var valueProp = property.FindPropertyRelative("_value");
            var enumLabels = new List<string>(valueProp.enumNames.Length + 1);
            var enumValues = new List<int>(valueProp.enumNames.Length);
            var enumIndices = new List<int>(valueProp.enumNames.Length);

            enumLabels.Add("None");
            var selectedIndex = 0;
            for (var i = 0; i < valueProp.enumNames.Length; i++) {
                var enumLabel = valueProp.enumNames[i];
                var enumValue = (int)Enum.Parse(enumType, enumLabel);
                
                // 同じ値が既にあれば合成
                var foundIndex = enumValues.IndexOf(enumValue) + 1;
                if (foundIndex > 0) {
                    enumLabels[foundIndex] = $"{enumLabels[foundIndex]}, {enumLabel}";
                    continue;
                }

                // 選択状態
                if (hasValueProp.boolValue && i == valueProp.enumValueIndex) {
                    selectedIndex = enumValues.Count + 1;
                }
                
                enumValues.Add(enumValue);
                enumIndices.Add(i);
                enumLabels.Add(valueProp.enumDisplayNames[i]);
            }

            using (new EditorGUI.PropertyScope(position, label, property)) {
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, enumLabels.ToArray());
                    if (scope.changed) {
                        if (selectedIndex == 0) {
                            hasValueProp.boolValue = false;
                        }
                        else {
                            hasValueProp.boolValue = true;
                            valueProp.enumValueIndex = enumIndices[selectedIndex - 1];
                        }
                    }
                }
            }
        }
    }
}