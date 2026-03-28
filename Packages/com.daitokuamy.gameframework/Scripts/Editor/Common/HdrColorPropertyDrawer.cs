using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// HdrColorのGUI拡張
    /// </summary>
    [CustomPropertyDrawer(typeof(HdrColor))]
    public sealed class HdrColorPropertyDrawer : PropertyDrawer {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var color = property.FindPropertyRelative("color");
            EditorGUI.ColorField(position, label, color.colorValue, true, true, true);
        }
    }
}
