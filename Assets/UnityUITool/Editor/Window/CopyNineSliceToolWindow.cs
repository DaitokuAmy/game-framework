using System;
using UnityEditor;
using UnityEngine;

namespace UnityUITool.Editor {
    /// <summary>
    /// 9Sliceをコピーするツール
    /// </summary>
    public class CopyNineSliceToolWindow : EditorWindow {
        [SerializeField]
        private Sprite _sourceSprite;
        [SerializeField]
        private Sprite[] _destinationSprites = Array.Empty<Sprite>();

        /// <summary>
        /// Windowを開く処理
        /// </summary>
        [MenuItem("Window/Unity UI Tool/Copy Nine Slice Tool")]
        private static void Open() {
            GetWindow<CopyNineSliceToolWindow>("Copy Nine Slice Tool");
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            var serializedObject = new SerializedObject(this);
            serializedObject.Update();

            var sourceSpriteProp = serializedObject.FindProperty("_sourceSprite");
            var destinationSpritesProp = serializedObject.FindProperty("_destinationSprites");
            EditorGUILayout.PropertyField(sourceSpriteProp, true);
            EditorGUILayout.PropertyField(destinationSpritesProp, true);

            serializedObject.ApplyModifiedProperties();

            using (new EditorGUI.DisabledScope(!CheckTargetSprite(_sourceSprite, out var sourceImporter) || _destinationSprites.Length <= 0)) {
                if (GUILayout.Button("Copy")) {
                    foreach (var destSprite in _destinationSprites) {
                        if (CheckTargetSprite(destSprite, out var destImporter)) {
                            destImporter.spriteBorder = sourceImporter.spriteBorder;
                            EditorUtility.SetDirty(destImporter);
                            destImporter.SaveAndReimport();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 対象のSpriteとなるかチェック
        /// </summary>
        private bool CheckTargetSprite(Sprite sprite, out TextureImporter importer) {
            importer = null;
            
            if (sprite == null) {
                return false;
            }
            
            importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
            if (importer == null) {
                return false;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single) {
                return false;
            }

            return true;
        }
    }
}