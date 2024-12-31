using System.Collections.Generic;
using SampleGame.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace SampleGame.Editor {
    /// <summary>
    /// ScriptableTableData用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(ScriptableTableData<>), true)]
    public class ScriptableTableDataEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Set automatically Id")) {
                if (EditorUtility.DisplayDialog("警告", "既存のIDをElementのIndexをベースに割り振り直しますか？", "OK", "Cancel")) {
                    serializedObject.Update();
                    var elementsProp = serializedObject.FindProperty("elements");
                    for (var i = 0; i < elementsProp.arraySize; i++) {
                        var elementProp = elementsProp.GetArrayElementAtIndex(i);
                        var idProp = elementProp.FindPropertyRelative("id");
                        if (idProp != null) {
                            idProp.intValue = i + 1;
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
                    if (listUpIds.Contains(id)) {
                        return true;
                    }
                    
                    listUpIds.Add(id);
                }
            }

            return false;
        }
    }
}