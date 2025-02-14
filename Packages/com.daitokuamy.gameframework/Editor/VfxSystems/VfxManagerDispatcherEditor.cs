using UnityEditor;
using UnityEngine;

namespace GameFramework.VfxSystems.Editor {
    /// <summary>
    /// VfxManagerDispatcherのEditor拡張
    /// </summary>
    [CustomEditor(typeof(VfxManagerDispatcher))]
    public class VfxManagerDispatcherEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            var dispatcher = target as VfxManagerDispatcher;
            var vfxManger = dispatcher != null ? dispatcher.Manager : null;

            if (vfxManger == null) {
                return;
            }

            // Poolのリセット
            using (new GUILayout.HorizontalScope()) {
                GUILayout.Label("Reset Pool", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button("Execute")) {
                    vfxManger.Clear();
                }
            }
        }
    }
}