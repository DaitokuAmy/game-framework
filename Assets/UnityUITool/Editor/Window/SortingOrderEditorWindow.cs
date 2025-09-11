using System;
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
    /// SortingOrderを一覧/編集するためのエディタウィンドウ
    /// </summary>
    public class SortingOrderEditorWindow : EditorWindow {
        /// <summary>
        /// 表示フィルター
        /// </summary>
        [Flags]
        private enum Filters {
            Canvas = 1 << 0,
            ParticleSystem = 1 << 1,
            SpriteRenderer = 1 << 2,
        }

        /// <summary>
        /// ソート可能オブジェクト情報
        /// </summary>
        private class SortableObjectInfo {
            public readonly List<SortableObjectInfo> Children = new();

            public string Path = string.Empty;
            public ISortableObject Target;
        }

        /// <summary>
        /// ソート可能オブジェクト用インターフェース
        /// </summary>
        private interface ISortableObject {
            int SortingLayerId { get; }

            bool CheckFilter(Filters filter);
            void DrawGUILayout();
        }

        /// <summary>
        /// Canvas用のソート実装
        /// </summary>
        private class CanvasObject : ISortableObject {
            private readonly Canvas _canvas;

            int ISortableObject.SortingLayerId => _canvas.sortingLayerID;

            public CanvasObject(Canvas canvas) {
                _canvas = canvas;
            }

            bool ISortableObject.CheckFilter(Filters filters) {
                return (filters & Filters.Canvas) != 0;
            }

            void ISortableObject.DrawGUILayout() {
                EditorGUILayout.ObjectField(_canvas, typeof(Canvas), true);
                
                var root = _canvas.rootCanvas == _canvas;

                if (!root) {
                    using (var changeCheckScope = new EditorGUI.ChangeCheckScope()) {
                        var overrideSorting = _canvas.overrideSorting;
                        overrideSorting = EditorGUILayout.Toggle("Override", overrideSorting);
                        if (changeCheckScope.changed) {
                            _canvas.overrideSorting = overrideSorting;
                            EditorUtility.SetDirty(_canvas);
                        }
                    }
                }

                if (root || _canvas.overrideSorting) {
                    using (var changeCheckScope = new EditorGUI.ChangeCheckScope()) {
                        var order = _canvas.sortingOrder;
                        order = EditorGUILayout.IntField("Order", order);
                        if (changeCheckScope.changed) {
                            _canvas.sortingOrder = order;
                            EditorUtility.SetDirty(_canvas);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renderer用のソート実装
        /// </summary>
        private class RendererObject : ISortableObject {
            private readonly Renderer _renderer;
            private readonly Component _source;

            int ISortableObject.SortingLayerId => _renderer.sortingLayerID;

            public RendererObject(Renderer renderer, Component source) {
                _renderer = renderer;
                _source = source;
            }

            bool ISortableObject.CheckFilter(Filters filters) {
                if (_source is ParticleSystem) {
                    return (filters & Filters.ParticleSystem) != 0;
                }

                if (_source is SpriteRenderer) {
                    return (filters & Filters.SpriteRenderer) != 0;
                }

                return false;
            }

            void ISortableObject.DrawGUILayout() {
                EditorGUILayout.ObjectField(_source, _source.GetType(), true);
                
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope()) {
                    var order = _renderer.sortingOrder;
                    order = EditorGUILayout.IntField("Order", order);
                    if (changeCheckScope.changed) {
                        _renderer.sortingOrder = order;
                        EditorUtility.SetDirty(_renderer);
                    }
                }
            }
        }


        private ObjectPool<SortableObjectInfo> _sortableObjectInfoPool;
        private SortableObjectInfo _rootSortableObjectInfo;
        private int _currentLayerId;
        private Filters _filters;
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
            _sortableObjectInfoPool = new ObjectPool<SortableObjectInfo>(() => new SortableObjectInfo(), actionOnRelease: x => {
                x.Target = null;
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
            ReleaseObjectInfo(_rootSortableObjectInfo);
            _rootSortableObjectInfo = null;
            
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            _sortableObjectInfoPool.Dispose();
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                // CurrentのLayerId変更
                using (var scope = new EditorGUI.ChangeCheckScope()) {
                    var currentName = SortingLayer.IDToName(_currentLayerId);
                    var labels = SortingLayer.layers.Select(x => x.name).ToArray();
                    var currentIndex = -1;
                    for (var i = 0; i < labels.Length; i++) {
                        if (labels[i] == currentName) {
                            currentIndex = i;
                            break;
                        }
                    }

                    currentIndex = EditorGUILayout.Popup(currentIndex, labels);
                    if (scope.changed) {
                        _currentLayerId = SortingLayer.NameToID(labels[currentIndex]);
                        RebuildCanvasInfos();
                    }
                }

                // Filter変更
                _filters = (Filters)EditorGUILayout.EnumFlagsField(_filters);

                EditorGUILayout.Space();
            }

            using (var scope = new EditorGUILayout.ScrollViewScope(_scroll, GUI.skin.box)) {
                // ObjectInfoの描画
                void DrawObjectInfo(SortableObjectInfo canvasInfo) {
                    if (canvasInfo.Target != null) {
                        var target = canvasInfo.Target;
                        if (target.CheckFilter(_filters)) {
                            using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                                target.DrawGUILayout();
                            }
                        }
                    }

                    foreach (var child in canvasInfo.Children) {
                        EditorGUI.indentLevel++;
                        DrawObjectInfo(child);
                        EditorGUI.indentLevel--;
                    }
                }

                foreach (var child in _rootSortableObjectInfo.Children) {
                    DrawObjectInfo(child);
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
            ReleaseObjectInfo(_rootSortableObjectInfo);

            _rootSortableObjectInfo = _sortableObjectInfoPool.Get();

            // ソート可能オブジェクト情報の回収
            void AddObjectInfo(StringBuilder path, Transform current, SortableObjectInfo parentObjectInfo, int filterLayerId) {
                var root = path.Length <= 0;
                path.Append(root ? current.name : $"/{current.name}");

                bool TryCanvasObject(Transform source, out ISortableObject result) {
                    result = null;
                    var canvas = source.GetComponent<Canvas>();
                    if (canvas == null) {
                        return false;
                    }

                    result = new CanvasObject(canvas);
                    return true;
                }

                bool TryRendererObject<T>(Transform trans, out ISortableObject result)
                    where T : Component {
                    result = null;
                    var source = trans.GetComponent<T>();
                    if (source == null) {
                        return false;
                    }

                    if (source is Renderer) {
                        result = new RendererObject(source as Renderer, source);
                    }
                    else {
                        var renderer = trans.GetComponent<Renderer>();
                        if (renderer != null) {
                            result = new RendererObject(renderer, source);
                        }
                    }

                    return result != null;
                }

                if (TryCanvasObject(current, out var target) ||
                    TryRendererObject<ParticleSystem>(current, out target) ||
                    TryRendererObject<SpriteRenderer>(current, out target)) {
                    if (filterLayerId == target.SortingLayerId) {
                        var info = _sortableObjectInfoPool.Get();
                        info.Target = target;
                        info.Path = path.ToString();
                        parentObjectInfo.Children.Add(info);
                        parentObjectInfo = info;
                    }
                }

                foreach (Transform child in current) {
                    AddObjectInfo(path, child, parentObjectInfo, filterLayerId);
                }
            }

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null) {
                var path = new StringBuilder();
                AddObjectInfo(path, prefabStage.prefabContentsRoot.transform, _rootSortableObjectInfo, _currentLayerId);
            }
            else {
                for (var i = 0; i < EditorSceneManager.loadedRootSceneCount; i++) {
                    var scene = SceneManager.GetSceneAt(i);
                    var path = new StringBuilder(scene.name);
                    var rootTransforms = scene.GetRootGameObjects().Select(x => x.transform).ToArray();
                    foreach (var child in rootTransforms) {
                        AddObjectInfo(path, child, _rootSortableObjectInfo, _currentLayerId);
                    }
                }
            }
        }

        /// <summary>
        /// ObjectInfoの解放（再帰的）
        /// </summary>
        private void ReleaseObjectInfo(SortableObjectInfo sortableObjectInfo) {
            if (sortableObjectInfo == null) {
                return;
            }

            foreach (var child in sortableObjectInfo.Children) {
                ReleaseObjectInfo(child);
            }

            _sortableObjectInfoPool.Release(sortableObjectInfo);
        }
    }
}