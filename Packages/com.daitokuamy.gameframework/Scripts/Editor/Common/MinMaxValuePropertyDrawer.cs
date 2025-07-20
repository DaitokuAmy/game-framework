using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor {
    /// <summary>
    /// MinMaxValueタイプのGUI拡張
    /// </summary>
    public abstract class MinMaxValueDrawer : PropertyDrawer {
        private static readonly GUIContent MinusIcon = EditorGUIUtility.IconContent("d_Toolbar Minus");
        private static readonly GUIContent PlusIcon = EditorGUIUtility.IconContent("d_Toolbar Plus");
        private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float LineSpace = EditorGUIUtility.standardVerticalSpacing;
        private static readonly float Indent = 15.0f;

        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var lineRect = position;
            lineRect.height = LineHeight;

            var leftBox = lineRect;
            var rightBox = lineRect;
            leftBox.xMax -= 30;
            rightBox.width = 30;
            rightBox.x = leftBox.xMax;

            // ランダム判定
            var useRandomProp = property.FindPropertyRelative("useRandom");

            // ランダムの時は専用にラベルを描画
            if (useRandomProp.boolValue) {
                EditorGUI.LabelField(leftBox, label);
                leftBox.y += leftBox.height;
            }

            // 乱数使用ボタン
            if (GUI.Button(rightBox, useRandomProp.boolValue ? MinusIcon : PlusIcon)) {
                useRandomProp.boolValue ^= true;
            }

            // 各種情報描画
            var minValueProp = property.FindPropertyRelative("minValue");
            var maxValueProp = property.FindPropertyRelative("maxValue");
            if (useRandomProp.boolValue) {
                leftBox.xMin += Indent;
                leftBox.xMax = lineRect.xMax;
                EditorGUI.PropertyField(leftBox, minValueProp, true);
                leftBox.y += EditorGUI.GetPropertyHeight(minValueProp, true) + LineSpace;
                EditorGUI.PropertyField(leftBox, maxValueProp, true);
                leftBox.y += EditorGUI.GetPropertyHeight(maxValueProp, true) + LineSpace;
            }
            else {
                EditorGUI.PropertyField(leftBox, minValueProp, label, true);
                leftBox.y += EditorGUI.GetPropertyHeight(minValueProp, true) + LineSpace;
            }
        }

        /// <summary>
        /// 高さ計算
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // 乱数使用時は2行 + 1行
            var useRandomProp = property.FindPropertyRelative("useRandom");
            if (useRandomProp.boolValue) {
                var propertyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("minValue"), true);
                return LineHeight + (propertyHeight + LineSpace) * 2;
            }
            // そうでなければラベルを置き換える
            else {
                var propertyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("minValue"), label, true);
                return propertyHeight + LineSpace;
            }
        }
    }

    /// <summary>
    /// MinMaxFloat用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxFloat))]
    public class MinMaxFloatDrawer : MinMaxValueDrawer {
    }

    /// <summary>
    /// MinMaxVector2用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxVector2))]
    public class MinMaxVector2Drawer : MinMaxValueDrawer {
    }

    /// <summary>
    /// MinMaxVector3用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxVector3))]
    public class MinMaxVector3Drawer : MinMaxValueDrawer {
    }

    /// <summary>
    /// MinMaxVector4用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxVector4))]
    public class MinMaxVector4Drawer : MinMaxValueDrawer {
    }

    /// <summary>
    /// MinMaxAnimationCurve用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxAnimationCurve))]
    public class MinMaxAnimationCurveDrawer : MinMaxValueDrawer {
    }
}