using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Core.Editor {
    /// <summary>
    /// VisibleFlagAttribute用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(VisibleFlagAttribute))]
    public class VisibleFlagPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var visible = GetVisible(property);

            if (visible) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        /// <summary>
        /// 高さ取得
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var visible = GetVisible(property);

            if (visible) {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0.0f;
        }

        /// <summary>
        /// 表示状態の取得
        /// </summary>
        private bool GetVisible(SerializedProperty property) {
            var visibleFlagAttribute = attribute as VisibleFlagAttribute;
            if (visibleFlagAttribute == null) {
                return false;
            }
            
            var parentProperty = FindParentProperty(property);
            var targetProperty = default(SerializedProperty);
            if (parentProperty != null) {
                targetProperty = parentProperty.FindPropertyRelative(visibleFlagAttribute.FlagPropertyName);
            }
            else {
                targetProperty = property.serializedObject.FindProperty(visibleFlagAttribute.FlagPropertyName);
            }

            return targetProperty != null && targetProperty.boolValue;
        }

        /// <summary>
        /// 親のSerializedPropertyを検索する
        /// </summary>
        private SerializedProperty FindParentProperty(SerializedProperty serializedProperty) {
            var propertyPaths = serializedProperty.propertyPath.Split('.');
            if (propertyPaths.Length <= 1) {
                return default;
            }

            var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(propertyPaths.First());
            for (var index = 1; index < propertyPaths.Length - 1; index++) {
                if (propertyPaths[index] == "Array" && propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$")) {
                    var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                    var arrayIndex = int.Parse(match.Groups[1].Value);
                    parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                    index++;
                }
                else {
                    parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
                }
            }

            return parentSerializedProperty;
        }
    }
}