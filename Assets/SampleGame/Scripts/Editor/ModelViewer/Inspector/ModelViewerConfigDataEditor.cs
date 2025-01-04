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
            var actorAssetKeys = new HashSet<string>();
            var actorSetupDataGuids = AssetDatabase.FindAssets($"t:{nameof(ModelViewerPreviewActorSetupData)}");
            foreach (var guid in actorSetupDataGuids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var assetKey = fileName.Replace("dat_preview_actor_setup_", "");
                actorAssetKeys.Add(assetKey);
            }

            var sortedActorAssetKeys = actorAssetKeys.OrderBy(x => x).ToArray();

            serializedObject.Update();
            var masterProp = serializedObject.FindProperty("master");
            
            // ActorAssetKeysの更新
            var actorAssetKeysProp = masterProp.FindPropertyRelative("actorAssetKeys");
            actorAssetKeysProp.arraySize = sortedActorAssetKeys.Length;
            for (var i = 0; i < sortedActorAssetKeys.Length; i++) {
                actorAssetKeysProp.GetArrayElementAtIndex(i).stringValue = sortedActorAssetKeys[i];
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}