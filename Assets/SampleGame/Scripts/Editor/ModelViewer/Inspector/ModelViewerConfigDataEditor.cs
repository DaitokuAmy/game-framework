using System.Collections.Generic;
using System.IO;
using System.Linq;
using SampleGame.Infrastructure.ModelViewer;
using UnityEditor;
using UnityEngine;

namespace SampleGame.Editor.ModelViewer {
    /// <summary>
    /// ModelViewerConfigData用のエディタ拡張
    /// </summary>
    [CustomEditor(typeof(ModelViewerConfigData))]
    public class ModelViewerConfigDataEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Update Master")) {
                UpdateMaster();
            }
        }

        /// <summary>
        /// マスター情報の更新
        /// </summary>
        private void UpdateMaster() {
            void UpdateSetupData<T>(string prefix, string assetKeysPropertyName) {
                var assetKeys = new HashSet<string>();
                var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    var assetKey = fileName.Replace(prefix, "");
                    assetKeys.Add(assetKey);
                }

                var sortedAssetKeys = assetKeys.OrderBy(x => x).ToArray();

                serializedObject.Update();
                var masterProp = serializedObject.FindProperty("master");
            
                // AssetKeysの更新
                var assetKeysProp = masterProp.FindPropertyRelative(assetKeysPropertyName);
                assetKeysProp.arraySize = sortedAssetKeys.Length;
                for (var i = 0; i < sortedAssetKeys.Length; i++) {
                    assetKeysProp.GetArrayElementAtIndex(i).stringValue = sortedAssetKeys[i];
                }

                serializedObject.ApplyModifiedProperties();
            }
            
            UpdateSetupData<ModelViewerActorSetupData>("dat_model_viewer_actor_setup_", "actorAssetKeys");
            UpdateSetupData<ModelViewerEnvironmentSetupData>("dat_model_viewer_environment_setup_", "environmentAssetKeys");
        }
    }
}