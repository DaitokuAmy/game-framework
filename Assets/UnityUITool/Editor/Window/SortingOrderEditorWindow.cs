using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace UnityUITool.Editor {
    /// <summary>
    /// CanvasにあるSortingOrderを一覧/編集するためのエディタウィンドウ
    /// </summary>
    public class SortingOrderEditorWindow : EditorWindow {
        /// <summary>
        /// キャンバス情報
        /// </summary>
        private class CanvasInfo {
            public string Path = string.Empty;
            public Canvas Canvas;
            public List<CanvasInfo> Children = new();
        }

        private ObjectPool<CanvasInfo> _canvasInfoPool;
        private CanvasInfo _rootCanvasInfo = new();
        private Vector2 _scroll;

        /// <summary>
        /// Windowを開く処理
        /// </summary>
        [MenuItem("Window/Unity UI Tool/Sorting Order Editor")]
        private static void Open() {
            GetWindow<SortingOrderEditorWindow>("Sorting Order Editor");
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            _canvasInfoPool = new ObjectPool<CanvasInfo>(() => new CanvasInfo(), actionOnRelease: x => {
                x.Canvas = null;
                x.Path = string.Empty;
                x.Children.Clear();
            });
            
            ObjectChangeEvents.changesPublished += OnChangesPublished;
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            RebuildCanvasInfos();
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            
            _canvasInfoPool.Dispose();
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, GUI.skin.box)) {
                // CanvasInfoの描画
                void DrawCanvasInfo(CanvasInfo canvasInfo) {
                    if (canvasInfo.Canvas != null) {
                        var canvas = canvasInfo.Canvas;
                        var root = canvas.rootCanvas == canvas;
                        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                            var displayName = $"{canvas.name}";
                            EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
                            if (!root) {
                                using (var changeCheckScope = new EditorGUI.ChangeCheckScope()) {
                                    var overrideSorting = canvas.overrideSorting;
                                    overrideSorting = EditorGUILayout.Toggle("Override", overrideSorting);
                                    if (changeCheckScope.changed) {
                                        canvas.overrideSorting = overrideSorting;
                                        EditorUtility.SetDirty(canvas);
                                    }
                                }
                            }

                            if (root || canvas.overrideSorting) {
                                using (var changeCheckScope = new EditorGUI.ChangeCheckScope()) {
                                    var order = canvas.sortingOrder;
                                    order = EditorGUILayout.IntField("Order", order);
                                    if (changeCheckScope.changed) {
                                        canvas.sortingOrder = order;
                                        EditorUtility.SetDirty(canvas);
                                    }
                                }
                            }
                        }
                    }

                    foreach (var child in canvasInfo.Children) {
                        EditorGUI.indentLevel++;
                        DrawCanvasInfo(child);
                        EditorGUI.indentLevel--;
                    }
                }

                foreach (var child in _rootCanvasInfo.Children) {
                    DrawCanvasInfo(child);
                }

                _scroll = scope.scrollPosition;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            Repaint();
        }

        /// <summary>
        /// Sceneの状態変化した時
        /// </summary>
        private void OnChangesPublished(ref ObjectChangeEventStream stream) {
            RebuildCanvasInfos();
        }

        /// <summary>
        /// PrefabModeに入った時
        /// </summary>
        private void OnPrefabStageOpened(PrefabStage stage) {
            RebuildCanvasInfos();
        }

        /// <summary>
        /// PrefabModeを抜けた時
        /// </summary>
        private void OnPrefabStageClosing(PrefabStage stage) {
            RebuildCanvasInfos();
        }

        /// <summary>
        /// Sceneオープン時
        /// </summary>
        private void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            RebuildCanvasInfos();
        }

        /// <summary>
        /// Scene読み込み時
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            RebuildCanvasInfos();
        }

        /// <summary>
        /// CanvasInfoの再構築
        /// </summary>
        private void RebuildCanvasInfos() {
            ReleaseCanvasInfo(_rootCanvasInfo);

            // Canvas情報の回収
            _rootCanvasInfo = _canvasInfoPool.Get();

            void AddCanvasInfo(StringBuilder path, Transform current, CanvasInfo parentCanvasInfo) {
                var root = path.Length <= 0;
                path.Append(root ? current.name : $"/{current.name}");
                var canvas = current.GetComponent<Canvas>();
                if (canvas != null) {
                    var canvasInfo = _canvasInfoPool.Get();
                    canvasInfo.Canvas = canvas;
                    canvasInfo.Path = path.ToString();
                    parentCanvasInfo.Children.Add(canvasInfo);
                    parentCanvasInfo = canvasInfo;
                }

                foreach (Transform child in current) {
                    AddCanvasInfo(path, child, parentCanvasInfo);
                }
            }

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null) {
                var path = new StringBuilder();
                AddCanvasInfo(path, prefabStage.prefabContentsRoot.transform, _rootCanvasInfo);
            }
            else {
                for (var i = 0; i < EditorSceneManager.loadedRootSceneCount; i++) {
                    var scene = SceneManager.GetSceneAt(i);
                    var path = new StringBuilder(scene.name);
                    var rootTransforms = scene.GetRootGameObjects().Select(x => x.transform).ToArray();
                    foreach (var child in rootTransforms) {
                        AddCanvasInfo(path, child, _rootCanvasInfo);
                    }
                }
            }
        }

        /// <summary>
        /// CanvasInfoの解放（再帰的）
        /// </summary>
        private void ReleaseCanvasInfo(CanvasInfo canvasInfo) {
            if (canvasInfo == null) {
                return;
            }

            foreach (var child in canvasInfo.Children) {
                ReleaseCanvasInfo(child);
            }

            _canvasInfoPool.Release(canvasInfo);
        }
    }
}