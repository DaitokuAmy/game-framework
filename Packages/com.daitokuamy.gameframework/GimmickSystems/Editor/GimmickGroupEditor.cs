using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.GimmickSystems.Editor {
    /// <summary>
    /// GimmickPartsのエディタ拡張
    /// </summary>
    [CustomEditor(typeof(GimmickGroup))]
    public class GimmickGroupEditor : UnityEditor.Editor {
        // Gimmickのキーを表示するためのList
        private ReorderableList _gimmickInfoList;
        // 選択中Gimmickのエディタ描画用
        private UnityEditor.Editor _selectedGimmickEditor;
        // 出力先のPrefab
        private GameObject _exportPrefab;

        /// <summary>
        /// インスペクタ描画
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();

            _gimmickInfoList.DoLayoutList();

            // 選択中のGimmickがあればInspector描画
            if (_selectedGimmickEditor != null) {
                using (new EditorGUILayout.VerticalScope("Box")) {
                    _selectedGimmickEditor.OnInspectorGUI();
                }
            }

            serializedObject.ApplyModifiedProperties();

            // Prefabへの保存
            if (Application.isPlaying) {
                _exportPrefab = EditorGUILayout.ObjectField("Export Prefab", _exportPrefab, typeof(GameObject), false) as GameObject;
                if (_exportPrefab == null) {
                    if (GUILayout.Button("Search Prefab")) {
                        _exportPrefab = SearchPrefab(target as GimmickGroup);
                    }
                }

                using (new EditorGUI.DisabledScope(_exportPrefab == null)) {
                    if (GUILayout.Button("Export", GUILayout.Width(200))) {
                        ExportPrefab(target as GimmickGroup, _exportPrefab);
                    }
                }
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            var gimmickInfos = serializedObject.FindProperty("_gimmickInfos");
            _gimmickInfoList = new ReorderableList(serializedObject, gimmickInfos);

            // Gimmickの取得
            Gimmick GetGimmick(int index) {
                var elementProp = gimmickInfos.GetArrayElementAtIndex(index);
                var gimmick = elementProp.FindPropertyRelative("gimmick").objectReferenceValue;
                return gimmick as Gimmick;
            }

            // ヘッダー描画
            _gimmickInfoList.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, "Gimmicks"); };

            // 要素描画処理
            _gimmickInfoList.drawElementCallback += (rect, index, isActive, isFocused) => {
                var info = gimmickInfos.GetArrayElementAtIndex(index);
                var r = rect;
                r.width = 120.0f;
                info.FindPropertyRelative("key").stringValue = EditorGUI.TextField(r, info.FindPropertyRelative("key").stringValue);
                r.x += r.width;
                r.width = rect.width - r.width;
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUI.ObjectField(r, info.FindPropertyRelative("gimmick").objectReferenceValue, typeof(Gimmick), true);
                }
            };

            // 要素追加処理
            _gimmickInfoList.onAddCallback += list => {
                var gimmickParts = serializedObject.targetObject as GimmickGroup;
                if (gimmickParts == null) {
                    return;
                }

                var menu = new GenericMenu();
                var gimmickInfosProp = list.serializedProperty;
                var gimmickTypes = TypeCache.GetTypesDerivedFrom<Gimmick>()
                    .Where(x => !x.IsAbstract && !x.IsGenericType)
                    .ToArray();
                foreach (var gimmickType in gimmickTypes) {
                    var t = gimmickType;
                    var basePath = "Others/";
                    if (t.IsSubclassOf(typeof(ActiveGimmick))) {
                        basePath = "Active/";
                    }
                    else if (t.IsSubclassOf(typeof(AnimationGimmick))) {
                        basePath = "Animation/";
                    }
                    else if (t.IsSubclassOf(typeof(InvokeGimmick))) {
                        basePath = "Invoke/";
                    }
                    else if (t.IsSubclassOf(typeof(StateGimmick))) {
                        basePath = "State/";
                    }

                    menu.AddItem(new GUIContent($"{basePath}{t.Name}"), false, () => {
                        var gimmickRoot = serializedObject.FindProperty("_gimmickRoot").objectReferenceValue as GameObject;
                        if (gimmickRoot == null) {
                            Debug.LogError("Not found gimmickRoot");
                            return;
                        }

                        serializedObject.Update();
                        gimmickInfosProp.InsertArrayElementAtIndex(gimmickInfosProp.arraySize);
                        var elementProp = gimmickInfosProp.GetArrayElementAtIndex(gimmickInfosProp.arraySize - 1);
                        var gimmick = Undo.AddComponent(gimmickRoot, t);
                        elementProp.FindPropertyRelative("key").stringValue = "Empty";
                        elementProp.FindPropertyRelative("gimmick").objectReferenceValue = gimmick;
                        serializedObject.ApplyModifiedProperties();
                        _gimmickInfoList.Select(_gimmickInfoList.count - 1);
                    });
                }

                menu.ShowAsContext();
            };

            // 要素削除処理
            _gimmickInfoList.onRemoveCallback += list => {
                var gimmickParts = serializedObject.targetObject as GimmickGroup;
                if (gimmickParts == null) {
                    return;
                }

                // 選択中の物を全て消す
                var gimmickInfosProp = list.serializedProperty;
                for (var i = list.selectedIndices.Count - 1; i >= 0; i--) {
                    var index = list.selectedIndices[i];
                    var elementProp = gimmickInfosProp.GetArrayElementAtIndex(index);
                    var gimmick = elementProp.FindPropertyRelative("gimmick").objectReferenceValue;
                    if (gimmick != null) {
                        Undo.DestroyObjectImmediate(gimmick);
                    }

                    list.serializedProperty.DeleteArrayElementAtIndex(index);
                }

                list.ClearSelection();
                if (_selectedGimmickEditor != null) {
                    DestroyImmediate(_selectedGimmickEditor);
                    _selectedGimmickEditor = null;
                }
            };

            // 要素選択状態変更
            void SelectGimmick(IEnumerable<int> selectedIndices) {
                if (_selectedGimmickEditor != null) {
                    DestroyImmediate(_selectedGimmickEditor);
                    _selectedGimmickEditor = null;
                }

                var gimmicks = selectedIndices
                    .Select(GetGimmick)
                    .Select(x => (Object)x)
                    .ToArray();
                if (gimmicks.Length > 0) {
                    _selectedGimmickEditor = CreateEditor(gimmicks[0]);
                }
            }

            _gimmickInfoList.onSelectCallback += list => { SelectGimmick(list.selectedIndices); };

            // 初期状態で要素選択しておく
            _gimmickInfoList.ClearSelection();
            if (gimmickInfos.arraySize > 0) {
                _gimmickInfoList.Select(0);
                SelectGimmick(new[] { 0 });
            }
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            if (_selectedGimmickEditor != null) {
                DestroyImmediate(_selectedGimmickEditor);
                _selectedGimmickEditor = null;
            }
        }

        /// <summary>
        /// 出力先のPrefabを検索
        /// </summary>
        private GameObject SearchPrefab(GimmickGroup gimmickGroup) {
            var prefabName = gimmickGroup.name.Replace("(Clone)", "");
            var guids = AssetDatabase.FindAssets($"{prefabName} t:prefab");
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (asset.name != prefabName) {
                    continue;
                }

                var group = asset.GetComponent<GimmickGroup>();
                if (group != null) {
                    return asset;
                }
            }

            return null;
        }

        /// <summary>
        /// Prefabに出力
        /// </summary>
        private void ExportPrefab(GimmickGroup gimmickGroup, GameObject prefab) {
            Core.Editor.EditorUtility.EditPrefab(prefab, obj => {
                // var cameras = gimmickGroup.GetComponentsInChildren<CinemachineVirtualCameraBase>(true);
                // var components = gimmickGroup.GetComponentsInChildren<CinemachineComponentBase>(true);
                // var extensions = gimmickGroup.GetComponentsInChildren<CinemachineExtension>(true);
                // var cameraTargets = gimmickGroup.GetComponentsInChildren<CameraTarget>(true);
                //
                // // それぞれを対応する場所にコピー
                // void CopyComponents<T>(T[] sources, Func<T, GameObject, T> insertComponentFunc = null)
                //     where T : MonoBehaviour {
                //     foreach (var src in sources) {
                //         var path = GetTransformPath(gimmickGroup.transform, src.transform);
                //         var exportTarget = obj.transform.Find(path);
                //         if (exportTarget == null) {
                //             Debug.LogWarning($"Not found export path. [{path}]");
                //             continue;
                //         }
                //
                //         var dest = exportTarget.GetComponent(src.GetType());
                //         if (dest == null && insertComponentFunc != null) {
                //             dest = insertComponentFunc.Invoke(src, exportTarget.gameObject);
                //         }
                //
                //         if (dest == null) {
                //             Debug.LogWarning($"Not found export component. [{path}:{src.GetType()}]");
                //             continue;
                //         }
                //
                //         EditorUtility.CopySerializedIfDifferent(src, dest);
                //     }
                // }
                //
                // // Cameraに含まれるComponent/Extensionを全削除
                // void RemoveComponentsAndExtensions(CinemachineVirtualCameraBase[] cameras) {
                //     foreach (var cam in cameras) {
                //         var vcam = cam as CinemachineVirtualCamera;
                //         if (vcam == null) {
                //             continue;
                //         }
                //     
                //         var path = GetTransformPath(gimmickGroup.transform, vcam.transform);
                //         var exportTarget = obj.transform.Find(path);
                //         if (exportTarget == null) {
                //             Debug.LogWarning($"Not found export path. [{path}]");
                //             continue;
                //         }
                //
                //         var exportVcam = exportTarget.GetComponent<CinemachineVirtualCamera>();
                //         if (exportVcam == null) {
                //             Debug.LogWarning($"Not found export virtual camera. [{path}]");
                //             continue;
                //         }
                //         
                //         // Componentを全部削除
                //         exportVcam.DestroyCinemachineComponent<CinemachineComponentBase>();
                //         // Extensionを全部削除
                //         var extList = exportVcam.GetComponentsInChildren<CinemachineExtension>(true);
                //         foreach (var ext in extList) {
                //             exportVcam.RemoveExtension(ext);
                //             DestroyImmediate(ext);
                //         }
                //     }
                // }
                //
                // CopyComponents(cameras);
                // RemoveComponentsAndExtensions(cameras);
                // CopyComponents(components, (x, y) => {
                //     var prevComponents = y.GetComponents<CinemachineComponentBase>();
                //     foreach (var prevComponent in prevComponents) {
                //         if (prevComponent.Stage == x.Stage) {
                //             // 同じStageのものは削除
                //             DestroyImmediate(prevComponent);
                //         }
                //     }
                //
                //     // 出力先Componentがなければ追加する
                //     return y.AddComponent(x.GetType()) as CinemachineComponentBase;
                // });
                // CopyComponents(extensions, (x, y) => {
                //     // 出力先Componentがなければ追加する
                //     return y.AddComponent(x.GetType()) as CinemachineExtension;
                // });
                // CopyComponents(cameraTargets, (x, y) => {
                //     // 出力先Componentがなければ追加する
                //     return y.AddComponent<CameraTarget>();
                // });
            });

            EditorUtility.SetDirty(prefab);
            AssetDatabase.Refresh();
        }
    }
}