using System;
using System.Linq;
using System.Reflection;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace GameFramework.CameraSystems.Editor {
    /// <summary>
    /// CameraGroupのエディタ拡張
    /// </summary>
    [CustomEditor(typeof(CameraGroup))]
    public class CameraGroupEditor : UnityEditor.Editor {
        // 出力先のPrefab
        private GameObject _exportPrefab;
        // 出力元のルート
        private Transform _targetRoot;

        /// <summary>
        /// インスペクタ描画
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            // Prefabへの保存
            if (Application.isPlaying) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("[Export Settings]", EditorStyles.boldLabel);
                _targetRoot = EditorGUILayout.ObjectField("Source Root", _targetRoot, typeof(Transform), true) as Transform;
                _exportPrefab = EditorGUILayout.ObjectField("Export Prefab", _exportPrefab, typeof(GameObject), false) as GameObject;
                if (_exportPrefab == null) {
                    if (GUILayout.Button("Search Prefab")) {
                        _exportPrefab = SearchPrefab(target as CameraGroup);
                    }
                }

                using (new EditorGUI.DisabledScope(_exportPrefab == null)) {
                    if (GUILayout.Button("Export", GUILayout.Width(200))) {
                        ExportPrefab(target as CameraGroup, _exportPrefab, _targetRoot);
                    }
                }
            }
        }

        /// <summary>
        /// 出力先のPrefabを検索
        /// </summary>
        private GameObject SearchPrefab(CameraGroup cameraGroup) {
            var guids = AssetDatabase.FindAssets($"{cameraGroup.DefaultName} t:prefab");
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (asset.name != cameraGroup.DefaultName) {
                    continue;
                }

                var group = asset.GetComponent<CameraGroup>();
                if (group != null) {
                    return asset;
                }
            }

            return null;
        }

        /// <summary>
        /// Prefabに出力
        /// </summary>
        private void ExportPrefab(CameraGroup cameraGroup, GameObject prefab, Transform targetRoot) {
            Core.Editor.EditorUtility.EditPrefab(prefab, obj => {
                if (!CheckInParent(targetRoot, cameraGroup.transform)) {
                    targetRoot = cameraGroup.transform;
                }

                var cameras = targetRoot.GetComponentsInChildren<CinemachineVirtualCameraBase>(true);
                var components = targetRoot.GetComponentsInChildren<CinemachineComponentBase>(true);
                var extensions = targetRoot.GetComponentsInChildren<CinemachineExtension>(true);
                var cameraTargets = targetRoot.GetComponentsInChildren<CameraTarget>(true);
                var cameraComponents = targetRoot.GetComponentsInChildren<ICameraComponent>(true);

                // それぞれを対応する場所にコピー
                void CopyComponents<T>(T[] sources, Func<T, GameObject, T> insertComponentFunc = null)
                    where T : MonoBehaviour {
                    foreach (var src in sources) {
                        var path = Core.Editor.EditorUtility.GetTransformPath(cameraGroup.transform, src.transform);
                        var exportTarget = obj.transform.Find(path);
                        if (exportTarget == null) {
                            // 階層が存在しなければ作成する
                            var childNames = path.Split("/");
                            exportTarget = obj.transform;
                            var baseTrans = cameraGroup.transform;
                            foreach (var childName in childNames) {
                                var parent = exportTarget;
                                exportTarget = exportTarget.Find(childName);
                                baseTrans = baseTrans.Find(childName);
                                if (exportTarget == null) {
                                    exportTarget = new GameObject(childName).transform;
                                    exportTarget.SetParent(parent, false);
                                    exportTarget.SetSiblingIndex(baseTrans.GetSiblingIndex());
                                }
                            }
                        }

                        var dest = exportTarget.GetComponent(src.GetType());
                        if (dest == null && insertComponentFunc != null) {
                            dest = insertComponentFunc.Invoke(src, exportTarget.gameObject);
                        }

                        if (dest == null) {
                            Debug.LogWarning($"Not found export component. [{path}:{src.GetType()}]");
                            continue;
                        }

                        // 差分がなければスキップ
                        if (!Core.Editor.EditorUtility.CheckDiffSerializedObject(new SerializedObject(src), new SerializedObject(dest), (a, b) => {
                                var fieldInfo = Core.Editor.EditorUtility.GetFieldInfo(a);
                                var attribute = fieldInfo.GetCustomAttribute(typeof(NoSaveDuringPlayAttribute));
                                var result = attribute != null;
                                return result;
                            })) {
                            continue;
                        }

                        // 差分が出てる所を編集
                        EditorUtility.CopySerializedIfDifferent(src, dest);
                        Debug.Log($"Exported Component. [{dest.GetType().Name}]{dest.name}");
                    }
                }

                // Cameraに含まれないComponent/Extensionを全削除
                void RemoveComponentsAndExtensions(CinemachineVirtualCameraBase[] cameras) {
                    foreach (var cam in cameras) {
                        var vcam = cam as CinemachineVirtualCamera;
                        if (vcam == null) {
                            continue;
                        }

                        var path = Core.Editor.EditorUtility.GetTransformPath(cameraGroup.transform, vcam.transform);
                        var exportTarget = obj.transform.Find(path);
                        if (exportTarget == null) {
                            Debug.LogWarning($"Not found export path. [{path}]");
                            continue;
                        }

                        var exportVcam = exportTarget.GetComponent<CinemachineVirtualCamera>();
                        if (exportVcam == null) {
                            Debug.LogWarning($"Not found export virtual camera. [{path}]");
                            continue;
                        }

                        // Componentの種類チェック
                        var sourceComponent = vcam.GetCinemachineComponent<CinemachineComponentBase>();
                        var exportComponent = exportVcam.GetCinemachineComponent<CinemachineComponentBase>();
                        if (exportComponent != null && sourceComponent.GetType() != exportComponent.GetType()) {
                            // 違っていたら削除する
                            exportVcam.DestroyCinemachineComponent<CinemachineComponentBase>();
                        }

                        // Extensionを全部削除
                        var sourceExtensions = vcam.GetComponentsInChildren<CinemachineExtension>(true);
                        var exportExtensions = exportVcam.GetComponentsInChildren<CinemachineExtension>(true);
                        foreach (var ext in exportExtensions) {
                            var contains = sourceExtensions.Any(x => x.GetType() == ext.GetType());
                            if (contains) {
                                continue;
                            }

                            // 含まれてない場合は削除
                            exportVcam.RemoveExtension(ext);
                            DestroyImmediate(ext);
                        }
                    }
                }

                CopyComponents(cameras, (x, y) => {
                    var prevCamera = y.GetComponent<CinemachineVirtualCameraBase>();
                    if (prevCamera != null) {
                        // 既に存在していたら削除
                        DestroyImmediate(prevCamera);
                    }

                    // 出力先Cameraがなければ追加する
                    return y.AddComponent(x.GetType()) as CinemachineVirtualCameraBase;
                });
                RemoveComponentsAndExtensions(cameras);
                CopyComponents(components, (x, y) => {
                    var prevComponents = y.GetComponents<CinemachineComponentBase>();
                    foreach (var prevComponent in prevComponents) {
                        if (prevComponent.Stage == x.Stage) {
                            // 同じStageのものは削除
                            DestroyImmediate(prevComponent);
                        }
                    }

                    // 出力先Componentがなければ追加する
                    return y.AddComponent(x.GetType()) as CinemachineComponentBase;
                });
                CopyComponents(extensions, (x, y) => {
                    // 出力先Componentがなければ追加する
                    return y.AddComponent(x.GetType()) as CinemachineExtension;
                });
                CopyComponents(cameraTargets, (x, y) => {
                    // 出力先Componentがなければ追加する
                    return y.AddComponent<CameraTarget>();
                });
                CopyComponents(cameraComponents.OfType<MonoBehaviour>().ToArray(), (x, y) => {
                    // 出力先Componentがなければ追加する
                    return y.AddComponent(x.GetType()) as MonoBehaviour;
                });
            });

            EditorUtility.SetDirty(prefab);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 対象のParentの子かチェックする
        /// </summary>
        private bool CheckInParent(Transform child, Transform parent) {
            if (child == null) {
                return false;
            }

            while (child.parent != null) {
                if (child.parent == parent) {
                    return true;
                }

                child = child.parent;
            }

            return false;
        }
    }
}