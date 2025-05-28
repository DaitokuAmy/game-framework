using UnityEditor;
using UnityEngine;

namespace GameFramework.UISystems.Editor {
    /// <summary>
    /// ComponentSelectorAttribute用のインスペクタ拡張
    /// </summary>
    [CustomPropertyDrawer(typeof(UIAnimationComponent))]
    public class UIAnimationComponentPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var leftRect = position;
            var rightRect = position;
            var space = 2;
            rightRect.xMin = rightRect.xMax - 22;
            leftRect.xMax = rightRect.xMin - space;

            var component = property.objectReferenceValue as UIAnimationComponent;

            // 通常のシリアライズ描画
            if (component != null) {
                label.text = $"{label.text}({component.Label})";
            }

            EditorGUI.PropertyField(leftRect, property, label);

            // 補助メニュー
            using (new EditorGUI.DisabledScope(component == null)) {
                var texture = EditorGUIUtility.FindTexture("d_icon dropdown@2x");
                if (GUI.Button(rightRect, texture) && component != null) {
                    var targetGameObject = component.gameObject;
                    var selectableComponents = targetGameObject.GetComponents<UIAnimationComponent>();
                    var menu = new GenericMenu();
                    foreach (var selectableComponent in selectableComponents) {
                        var c = selectableComponent;
                        menu.AddItem(new GUIContent($"{selectableComponent.Label}"), false, () => {
                            property.serializedObject.Update();
                            property.objectReferenceValue = c;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }

                    menu.ShowAsContext();
                }
            }
        }
    }
}