using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.UISystems.Editor {
    /// <summary>
    /// RecyclableScrollListのエディタ拡張
    /// </summary>
    [CustomEditor(typeof(RecyclableScrollList))]
    public class RecyclableScrollListEditor : UnityEditor.Editor {
        private SerializedProperty _scrollRectProp;
        private SerializedProperty _templateInfosProp;
        
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            serializedObject.Update();

            if (GUILayout.Button("Set Template Size")) {
                var scrollRect = _scrollRectProp.objectReferenceValue as ScrollRect;
                if (scrollRect != null) {
                    var vertical = scrollRect.vertical;
                    for (var i = 0; i < _templateInfosProp.arraySize; i++) {
                        var elementProp = _templateInfosProp.GetArrayElementAtIndex(i);
                        var templateProp = elementProp.FindPropertyRelative("template");
                        var sizeProp = elementProp.FindPropertyRelative("size");
                        var rectTrans = templateProp.objectReferenceValue as RectTransform;
                        if (rectTrans != null) {
                            sizeProp.floatValue = vertical ? rectTrans.sizeDelta.y : rectTrans.sizeDelta.x;
                        }
                    }
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _scrollRectProp = serializedObject.FindProperty("_scrollRect");
            _templateInfosProp = serializedObject.FindProperty("_templateInfos");
            
            if (target is RecyclableScrollList list) {
                if (_scrollRectProp.objectReferenceValue == null) {
                    serializedObject.Update();
                    _scrollRectProp.objectReferenceValue = list.GetComponent<ScrollRect>();
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}