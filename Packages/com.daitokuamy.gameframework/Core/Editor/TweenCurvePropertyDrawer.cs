using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Core.Editor {
    /// <summary>
    /// TweenCurve型用のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(TweenCurve))]
    public class TweenCurvePropertyDrawer : PropertyDrawer {
        private bool _initialized;
        private GUIContent[] _typeLabels;

        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Initialize();

            var easeTypeProp = property.FindPropertyRelative("_easeType");
            var animationCurveProp = property.FindPropertyRelative("_animationCurve");
            var useAnimationCurveProp = property.FindPropertyRelative("_useAnimationCurve");

            var index = useAnimationCurveProp.boolValue ? _typeLabels.Length - 1 : easeTypeProp.intValue;
            var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var rect = position;
            rect.height = lineHeight;
            using (var scope = new EditorGUI.ChangeCheckScope()) {
                index = EditorGUI.Popup(rect, label, index, _typeLabels);
                if (scope.changed) {
                    if (index == _typeLabels.Length - 1) {
                        useAnimationCurveProp.boolValue = true;
                    }
                    else {
                        useAnimationCurveProp.boolValue = false;
                        easeTypeProp.intValue = index;
                    }
                }
            }

            if (useAnimationCurveProp.boolValue) {
                rect.y += lineHeight;
                EditorGUI.PropertyField(rect, animationCurveProp, new GUIContent(" "));
            }
        }

        /// <summary>
        /// プロパティの高さを取得
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var useAnimationCurveProp = property.FindPropertyRelative("_useAnimationCurve");
            var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (useAnimationCurveProp.boolValue) {
                return lineHeight * 2;
            }

            return lineHeight;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;

            var typeLabels = Enum.GetNames(typeof(EaseType)).ToList();
            typeLabels.Add("Custom");
            _typeLabels = typeLabels.Select(x => new GUIContent(x)).ToArray();
        }
    }
}