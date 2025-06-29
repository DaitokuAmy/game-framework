using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework.TableSystems.Editor {
    /// <summary>
    /// ScriptableTableData用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(ScriptableTableData<,>), true)]
    public class ScriptableTableDataEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            void SetIdProp(SerializedProperty prop, int index) {
                if (prop == null) {
                    return;
                }

                if (prop.propertyType == SerializedPropertyType.Integer) {
                    prop.intValue = index + 1;
                }
                else if (prop.propertyType == SerializedPropertyType.String) {
                    prop.stringValue = $"{index + 1}";
                }
                else if (prop.propertyType == SerializedPropertyType.Enum) {
                    prop.intValue = index + 1;
                }
            }

            if (GUILayout.Button("Set automatically Id")) {
                if (EditorUtility.DisplayDialog("警告", "既存のIDをElementのIndexをベースに割り振り直しますか？", "OK", "Cancel")) {
                    serializedObject.Update();
                    var elementsProp = serializedObject.FindProperty("elements");
                    for (var i = 0; i < elementsProp.arraySize; i++) {
                        var elementProp = elementsProp.GetArrayElementAtIndex(i);
                        var idProp = elementProp.FindPropertyRelative("id");
                        if (idProp != null) {
                            SetIdProp(idProp, i);
                        }
                        else {
                            idProp = elementProp.FindPropertyRelative("_id");
                            SetIdProp(idProp, i);
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (CheckIdConflict()) {
                EditorGUILayout.HelpBox("IDの重複が発生しています", MessageType.Error);
            }
        }

        /// <summary>
        /// IDのコンフリクトチェック
        /// </summary>
        private bool CheckIdConflict() {
            var elementsProp = serializedObject.FindProperty("elements");
            var listUpIds = new HashSet<int>();
            for (var i = 0; i < elementsProp.arraySize; i++) {
                var elementProp = elementsProp.GetArrayElementAtIndex(i);
                var idProp = elementProp.FindPropertyRelative("id");
                if (idProp != null) {
                    var id = idProp.intValue;
                    if (!listUpIds.Add(id)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}