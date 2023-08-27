using UnityEditor;
using UnityEngine;

namespace GameFramework.ProjectileSystems.Editor {
    /// <summary>
    /// ProjectileObjectManagerDispatcherのEditor拡張
    /// </summary>
    [CustomEditor(typeof(ProjectileObjectManagerDispatcher))]
    public class VfxManagerDispatcherEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            var dispatcher = target as ProjectileObjectManagerDispatcher;
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