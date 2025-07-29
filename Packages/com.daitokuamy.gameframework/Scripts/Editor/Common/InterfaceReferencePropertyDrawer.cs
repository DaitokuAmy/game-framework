using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// InterfaceReference型用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(InterfaceReferenceBase), true)]
    public class InterfaceReferencePropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var targetProp = property.FindPropertyRelative("_target");

            var interfaceType = GetInterfaceTypeFromFieldInfo(fieldInfo);
            if (interfaceType == null) {
                EditorGUI.LabelField(position, label.text, "Invalid Interface Type");
                return;
            }

            label.text = $"{label.text} ({interfaceType.Name})";

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            var result = EditorGUI.ObjectField(position, label, targetProp.objectReferenceValue, typeof(Component), true);
            if (EditorGUI.EndChangeCheck()) {
                var interfaceResult = default(Object);
                if (result is Component resultObj) {
                    interfaceResult = resultObj.GetComponent(interfaceType);
                }

                targetProp.objectReferenceValue = interfaceResult;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 高さ計算
        /// </summary>
        private System.Type GetInterfaceTypeFromFieldInfo(System.Reflection.FieldInfo info) {
            var type = info.FieldType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(InterfaceReference<>)) {
                return type.GetGenericArguments()[0];
            }

            return null;
        }
    }
}