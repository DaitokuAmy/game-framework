using System;
using UnityEditor;
using UnityEngine;

namespace SampleGame.Editor {
    /// <summary>
    /// GeneralActorAction用の拡張
    /// </summary>
    [CustomPropertyDrawer(typeof(GeneralActorAction))]
    public class GeneralActionPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            var rect = position;

            rect.height = lineHeight;
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label);
            rect.y += rect.height;

            if (property.isExpanded) {
                rect.xMin += 15;

                var actionTypeProp = property.FindPropertyRelative("actionType");
                rect.height = lineHeight;

                var actionTypes = (GeneralActorAction.ActionType[])Enum.GetValues(typeof(GeneralActorAction.ActionType));
                var actionType = actionTypes[actionTypeProp.enumValueIndex];

                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    EditorGUI.PropertyField(rect, actionTypeProp);
                    if (scope.changed) {
                        // todo:参照の整理は今後したい
                    }
                }

                rect.y += rect.height;


                var actionProp = GetActionProperty(property, actionType);
                if (actionProp != null) {
                    rect.height = EditorGUI.GetPropertyHeight(actionProp, true);
                    EditorGUI.PropertyField(rect, actionProp, true);
                    rect.y += rect.height;
                }

                rect.xMin -= 15;
            }
        }

        /// <summary>
        /// 高さの取得
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var totalHeight = 0.0f;
            var actionTypeProp = property.FindPropertyRelative("actionType");
            var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;

            totalHeight += lineHeight;

            if (!property.isExpanded) {
                return totalHeight;
            }

            totalHeight += lineHeight;

            var actionTypes = (GeneralActorAction.ActionType[])Enum.GetValues(typeof(GeneralActorAction.ActionType));
            var actionType = actionTypes[actionTypeProp.enumValueIndex];
            var actionProp = GetActionProperty(property, actionType);
            if (actionProp != null) {
                totalHeight += EditorGUI.GetPropertyHeight(actionProp, property.isExpanded);
            }

            return totalHeight;
        }

        /// <summary>
        /// ActionPropertyの取得
        /// </summary>
        private SerializedProperty GetActionProperty(SerializedProperty property, GeneralActorAction.ActionType actionType) {
            switch (actionType) {
                case GeneralActorAction.ActionType.Clip: {
                    return property.FindPropertyRelative("clipActorAction");
                }
                case GeneralActorAction.ActionType.SequentialClip: {
                    return property.FindPropertyRelative("sequentialClipActorAction");
                }
                case GeneralActorAction.ActionType.Controller: {
                    return property.FindPropertyRelative("controllerActorAction");
                }
                case GeneralActorAction.ActionType.Timeline: {
                    return property.FindPropertyRelative("timelineActorAction");
                }
            }

            return null;
        }
    }
}