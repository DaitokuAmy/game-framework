using UnityEditor;
using UnityEngine;

namespace GameFramework.RendererSystems.Editor {
    /// <summary>
    /// HdrColorのGUI拡張
    /// </summary>
    [CustomPropertyDrawer(typeof(HdrColor))]
    public class HdrColorPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var color = property.FindPropertyRelative("color");
            EditorGUI.ColorField(position, label, color.colorValue, true, true, true);
        }
    }
}