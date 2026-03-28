using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// RendererMaterialのGUI拡張
    /// </summary>
    [CustomPropertyDrawer(typeof(RendererMaterial))]
    public sealed class RendererMaterialPropertyDrawer : PropertyDrawer {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var renderer = property.FindPropertyRelative("renderer");
            var materialIndex = property.FindPropertyRelative("materialIndex");

            var rect = position;
            var height = EditorGUIUtility.singleLineHeight;
            var unitOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            rect.height = height;
            EditorGUI.LabelField(rect, label);
            rect.y += unitOffset;
            EditorGUI.indentLevel++;
            renderer.objectReferenceValue = EditorGUI.ObjectField(rect, renderer.displayName, renderer.objectReferenceValue, typeof(Renderer), true);
            var r = renderer.objectReferenceValue as Renderer;
            if (r != null) {
                rect.y += unitOffset;
                var materials = r.sharedMaterials;
                var labels = materials
                    .Select((x, i) => $"{i}:{x.name}")
                    .ToArray();
                var index = Mathf.Clamp(materialIndex.intValue, 0, materials.Length - 1);
                materialIndex.intValue = EditorGUI.Popup(rect, "Material", index, labels);
            }
            EditorGUI.indentLevel--;
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var renderer = property.FindPropertyRelative("renderer");
            var height = EditorGUIUtility.singleLineHeight;
            var unitOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var totalHeight = height + unitOffset;
            if (renderer.objectReferenceValue is Renderer) {
                totalHeight += unitOffset;
            }

            return totalHeight;
        }
    }
}
