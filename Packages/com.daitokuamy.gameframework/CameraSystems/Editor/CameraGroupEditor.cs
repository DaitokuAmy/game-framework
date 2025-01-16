using System;
using System.Linq;
using System.Reflection;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace GameFramework.CameraSystems.Editor {
    /// <summary>
    /// CameraGroupのエディタ拡張
    /// </summary>
    [CustomEditor(typeof(CameraGroup))]
    public class CameraGroupEditor : UnityEditor.Editor {
        // 出力元のルート
        private Transform _targetRoot;
        // 出力先のPrefab
        private GameObject _exportPrefab;
        // 出力先のPrefabから見た出力CameraGroupの相対Path
        private string _exportRootPath;

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
                if (_exportPrefab != null) {
                    _exportRootPath = EditorGUILayout.TextField("Export Root Path", _exportRootPath);
                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.Space(EditorGUIUtility.labelWidth);
                        if (GUILayout.Button("Search")) {
                            var menu = new GenericMenu();
                            var paths = _exportPrefab.GetComponentsInChildren<CameraGroup>(true)
                                .Where(x => x.gameObject != _exportPrefab)
                                .Select(x => Core.Editor.EditorTool.GetTransformPath(_exportPrefab.transform, x.transform))
                                .ToArray();
                            for (var i = 0; i < paths.Length; i++) {
                                var path = paths[i];
                                menu.AddItem(new GUIContent(paths[i]), false, () => { _exportRootPath = path; });
                            }

                            menu.ShowAsContext();
                        }
                    }
                }

                if (_exportPrefab == null) {
                    if (GUILayout.Button("Search Prefab")) {
                        _exportPrefab = SearchPrefab(target as CameraGroup);
                    }
                }

                using (new EditorGUI.DisabledScope(_exportPrefab == null)) {
                    if (GUILayout.Button("Export", GUILayout.Width(200))) {
                        ExportPrefab(target as CameraGroup, _exportPrefab, _exportRootPath, _targetRoot);
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
        private void ExportPrefab(CameraGroup cameraGroup, GameObject prefab, string exportRootPath, Transform targetRoot) {
            Core.Editor.EditorTool.EditPrefab(prefab, obj => {
                if (!CheckInParent(targetRoot, cameraGroup.transform)) {
                    targetRoot = cameraGroup.transform;
                }

                // 出力先に相対パスが存在していたらそれを適用
                var exportRoot = obj.transform;
                if (!string.IsNullOrEmpty(exportRootPath)) {
                    var childNames = exportRootPath.Split('/');
                    foreach (var childName in childNames) {
                        var result = exportRoot.Find(childName);
                        if (result == null) {
                            break;
                        }

                        exportRoot = result;
                    }
                }

                var cameras = targetRoot.GetComponentsInChildren<CinemachineVirtualCameraBase>(true);
                var components = targetRoot.GetComponentsInChildren<CinemachineComponentBase>(true);
                var extensions = targetRoot.GetComponentsInChildren<CinemachineExtension>(true);
                var cameraTargets = targetRoot.GetComponentsInChildren<CameraTarget>(true);
                var cameraComponents = targetRoot.GetComponentsInChildren<ICameraComponent>(true);

                // それぞれを対応する場所にコピー
                void CopyComponents<T>(T[] sources, Func<SerializedProperty, bool> copyCheckFunc = null, Func<T, GameObject, T> insertComponentFunc = null)
                    where T : MonoBehaviour {
                    foreach (var src in sources) {
                        var path = Core.Editor.EditorTool.GetTransformPath(cameraGroup.transform, src.transform);
                        var exportTarget = exportRoot.Find(path);
                        if (exportTarget == null) {
                            // 階層が存在しなければ作成する
                            var childNames = path.Split("/");
                            exportTarget = exportRoot;
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

                        // 値の更新
                        Core.Editor.EditorTool.CopySerializedObject(src, dest, copyCheckFunc);
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

                        var path = Core.Editor.EditorTool.GetTransformPath(cameraGroup.transform, vcam.transform);
                        var exportTarget = exportRoot.Find(path);
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
                            DestroyImmediate(ext);
                        }
                    }
                }

                CopyComponents(cameras, prop => {
                    if (prop.propertyPath == "m_Follow" || prop.propertyPath == "m_LookAt") {
                        return false;
                    }

                    return true;
                }, (x, y) => {
                    var prevCamera = y.GetComponent<CinemachineVirtualCameraBase>();
                    var prevFollow = default(Transform);
                    var prevLookAt = default(Transform);
                    if (prevCamera != null) {
                        // Follow/LookAtは回収
                        prevFollow = prevCamera.Follow;
                        prevLookAt = prevCamera.LookAt;
                        // 既に別のCameraBaseが存在していたら削除
                        DestroyImmediate(prevCamera);
                    }

                    // 出力先Cameraがなければ追加する
                    var nextCamera = (CinemachineVirtualCameraBase)y.AddComponent(x.GetType());
                    nextCamera.Follow = prevFollow;
                    nextCamera.LookAt = prevLookAt;
                    return nextCamera;
                });
                RemoveComponentsAndExtensions(cameras);
                CopyComponents(components, null, (x, y) => {
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
                CopyComponents(extensions, null, (x, y) => {
                    // 出力先Componentがなければ追加する
                    return y.AddComponent(x.GetType()) as CinemachineExtension;
                });
                CopyComponents(cameraTargets, prop => {
                    if (prop.propertyPath == "_group") {
                        return false;
                    }

                    return true;
                }, (x, y) => {
                    // 出力先Componentがなければ追加する
                    return y.AddComponent<CameraTarget>();
                });
                // インスタンス上の参照先が入っているケースで消える事があるので一旦未対応
                // CopyComponents(cameraComponents.OfType<MonoBehaviour>().ToArray(), null, (x, y) => {
                //     // 出力先Componentがなければ追加する
                //     return y.AddComponent(x.GetType()) as MonoBehaviour;
                // });
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