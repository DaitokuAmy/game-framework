using UnityEditor;
using UnityEngine;

namespace GameFramework.ProjectileSystems.Editor {
    /// <summary>
    /// ProjectileObjectManagerDispatcherのEditor拡張
    /// </summary>
    [CustomEditor(typeof(ProjectileManagerDispatcher))]
    public class ProjectileManagerDispatcherEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            var dispatcher = target as ProjectileManagerDispatcher;
            var manager = dispatcher != null ? dispatcher.Manager : null;

            if (manager == null) {
                return;
            }

            // Poolのリセット
            using (new GUILayout.HorizontalScope()) {
                GUILayout.Label("Reset Pool", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button("Execute")) {
                    manager.Clear();
                }
            }
        }
    }
}