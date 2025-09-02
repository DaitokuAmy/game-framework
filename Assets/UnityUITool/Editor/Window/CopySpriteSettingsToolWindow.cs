using System;
using UnityEditor;
using UnityEngine;

namespace UnityUITool.Editor {
    /// <summary>
    /// Sprite設定をコピーするツール
    /// </summary>
    public class CopySpriteSettingsToolWindow : EditorWindow {
        /// <summary>
        /// コピーフィルター
        /// </summary>
        [Flags]
        private enum Filters {
            Boarder = 1 << 0,
            Pivot = 1 << 1,
            PixelsToUnit = 1 << 2,
        }
        
        /// <summary>
        /// オプション
        /// </summary>
        [Serializable]
        private class Option {
            [Tooltip("Boarderコピー時に中央からの値に変換するか")]
            public bool ReverseBoarder = false;
        }
        
        [SerializeField, Tooltip("コピー要素のフィルター")]
        private Filters _filters = Filters.Boarder | Filters.Pivot;
        [SerializeField, Tooltip("オプション")]
        private Option _option = new();
        [SerializeField, Tooltip("コピー元")]
        private Sprite _sourceSprite;
        [SerializeField, Tooltip("コピー先")]
        private Sprite[] _destinationSprites = Array.Empty<Sprite>();

        /// <summary>
        /// Windowを開く処理
        /// </summary>
        [MenuItem("Window/Unity UI Tool/Copy Sprite Settings Tool")]
        private static void Open() {
            GetWindow<CopySpriteSettingsToolWindow>("Copy Sprite Settings Tool");
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            var serializedObject = new SerializedObject(this);
            serializedObject.Update();

            var filtersProp = serializedObject.FindProperty("_filters");
            var optionProp = serializedObject.FindProperty("_option");
            var sourceSpriteProp = serializedObject.FindProperty("_sourceSprite");
            var destinationSpritesProp = serializedObject.FindProperty("_destinationSprites");
            EditorGUILayout.PropertyField(filtersProp);
            EditorGUILayout.PropertyField(optionProp, true);
            EditorGUILayout.PropertyField(sourceSpriteProp);
            EditorGUILayout.PropertyField(destinationSpritesProp, true);

            serializedObject.ApplyModifiedProperties();

            using (new EditorGUI.DisabledScope(!CheckTargetSprite(_sourceSprite, out var sourceImporter) || _destinationSprites.Length <= 0)) {
                if (GUILayout.Button("Copy")) {
                    foreach (var destSprite in _destinationSprites) {
                        if (CheckTargetSprite(destSprite, out var destImporter)) {
                            CopySpriteSettings(_filters, _option, sourceImporter, destImporter);
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

        /// <summary>
        /// Sprite設定のコピー
        /// </summary>
        private void CopySpriteSettings(Filters filters, Option option, TextureImporter sourceImporter, TextureImporter destinationImporter) {
            bool Check(Filters check) {
                return (filters & check) != 0;
            }
            
            if (Check(Filters.Boarder)) {
                if (option.ReverseBoarder) {
                    sourceImporter.GetSourceTextureWidthAndHeight(out var width, out var height);
                    var reverseBorder = sourceImporter.spriteBorder;
                    reverseBorder.x = width * 0.5f - reverseBorder.x;
                    reverseBorder.z = width * 0.5f - reverseBorder.z;
                    reverseBorder.y = height * 0.5f - reverseBorder.y;
                    reverseBorder.w = height * 0.5f - reverseBorder.w;

                    destinationImporter.GetSourceTextureWidthAndHeight(out var destWidth, out var destHeight);
                    var border = reverseBorder;
                    border.x = destWidth * 0.5f - border.x;
                    border.z = destWidth * 0.5f - border.z;
                    border.y = destHeight * 0.5f - border.y;
                    border.w = destHeight * 0.5f - border.w;
                    destinationImporter.spriteBorder = border;
                }
                else {
                    destinationImporter.spriteBorder = sourceImporter.spriteBorder;
                }
            }
            
            if (Check(Filters.Pivot)) {
                destinationImporter.spritePivot = sourceImporter.spritePivot;
            }
            
            if (Check(Filters.PixelsToUnit)) {
                destinationImporter.spritePixelsPerUnit = sourceImporter.spritePixelsPerUnit;
            }
        }
    }
}