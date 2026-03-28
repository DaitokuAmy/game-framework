using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// Percent型用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(Percent))]
    public sealed class PercentPropertyDrawer : PropertyDrawer {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var valueProp = property.FindPropertyRelative("RawValue");
            var current = valueProp.intValue / (float)Percent.UnitValue;

            label.text += " (Percent)";

            using (var scope = new EditorGUI.ChangeCheckScope()) {
                current = EditorGUI.FloatField(position, label, current);
                if (scope.changed) {
                    valueProp.intValue = (int)(current * Percent.UnitValue);
                }
            }
        }
    }
}
