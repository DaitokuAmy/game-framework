using GameFramework.Core;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// Permil型用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(Permil))]
    public class PermilPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var valueProp = property.FindPropertyRelative("_value");
            var current = valueProp.intValue / (float)Permil.One;

            label.text += " (Permil)";

            using (var scope = new EditorGUI.ChangeCheckScope()) {
                current = EditorGUI.FloatField(position, label, current);
                if (scope.changed) {
                    valueProp.intValue = (int)(current * Permil.One);
                }
            }
        }
    }
}