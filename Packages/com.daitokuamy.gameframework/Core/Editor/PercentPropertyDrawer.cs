using UnityEditor;
using UnityEngine;

namespace GameFramework.Core.Editor {
    /// <summary>
    /// Percent型用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(Percent))]
    public class PercentPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var valueProp = property.FindPropertyRelative("_value");
            var current = valueProp.intValue / (float)Percent.One;

            label.text += " (Percent)";

            using (var scope = new EditorGUI.ChangeCheckScope()) {
                current = EditorGUI.FloatField(position, label, current);
                if (scope.changed) {
                    valueProp.intValue = (int)(current * Percent.One);
                }
            }
        }
    }
}