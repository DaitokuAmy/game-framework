using UnityEditor;
using UnityEngine;

namespace GameFramework.CutsceneSystems.Editor {
    /// <summary>
    /// CutsceneManagerDispatcherのEditor拡張
    /// </summary>
    [CustomEditor(typeof(CutsceneManagerDispatcher))]
    public class CutsceneManagerDispatcherEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            var dispatcher = target as CutsceneManagerDispatcher;
            var cutsceneManger = dispatcher != null ? dispatcher.Manager : null;

            if (cutsceneManger == null) {
                return;
            }

            // Poolのリセット
            using (new GUILayout.HorizontalScope()) {
                GUILayout.Label("Reset Pool", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button("Execute")) {
                    cutsceneManger.Clear();
                }
            }
        }
    }
}