using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.UISystems {
    /// <summary>
    /// Uiの管理クラス
    /// </summary>
    public class UIManager : DisposableLateUpdatableTask {
        /// <summary>
        /// プレファブ管理用ハンドル(Disposeでアンロードされる)
        /// </summary>
        public struct AssetHandle : IProcess, IDisposable {
            private readonly Exception _exception;

            private UIManager _uIManager;
            private AssetInfo _assetInfo;

            /// <summary>読み込み完了しているか</summary>
            public bool IsDone => _assetInfo == null || _assetInfo.IsDone;
            /// <summary>有効か</summary>
            public bool IsValid => _assetInfo != null && Exception == null;
            /// <summary>エラー情報</summary>
            public Exception Exception => _exception ?? _assetInfo?.Exception;

            /// <summary>未使用</summary>
            object IEnumerator.Current => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal AssetHandle(UIManager uIManager, AssetInfo assetInfo) {
                _uIManager = uIManager;
                _assetInfo = assetInfo;
                _exception = null;
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal AssetHandle(Exception exception) {
                _uIManager = null;
                _assetInfo = null;
                _exception = exception;
            }

            /// <summary>
            /// 廃棄処理
            /// </summary>
            public void Dispose() {
                if (IsValid && _uIManager != null) {
                    _uIManager.RemoveAssetInfo(_assetInfo);
                    _assetInfo = null;
                    _uIManager = null;
                }
            }

            /// <summary>
            /// IEnumerator用
            /// </summary>
            bool IEnumerator.MoveNext() {
                return !IsDone;
            }

            /// <summary>
            /// IEnumerator用 - 未使用
            /// </summary>
            void IEnumerator.Reset() {
            }
        }

        /// <summary>
        /// アセット情報
        /// </summary>
        internal abstract class AssetInfo {
            public readonly List<Type> ServiceTypes = new();

            public bool Initialized;
            public Coroutine Coroutine;
            public Canvas[] RootCanvases;

            /// <summary>読み込み完了しているか</summary>
            public abstract bool IsDone { get; }
            /// <summary>エラー情報</summary>
            public abstract Exception Exception { get; }

            public abstract void Release();
        }

        /// <summary>
        /// シーン情報
        /// </summary>
        private class SceneInfo : AssetInfo {
            public string Key;
            public SceneAssetHandle AssetHandle;

            public override bool IsDone => Initialized && AssetHandle.IsDone;
            public override Exception Exception => AssetHandle.Exception;

            public override void Release() {
                SceneManager.UnloadSceneAsync(AssetHandle.Scene);
                AssetHandle.Release();
            }
        }

        /// <summary>
        /// プレファブ情報
        /// </summary>
        private class PrefabInfo : AssetInfo {
            public string Key;
            public AssetHandle<GameObject> AssetHandle;

            public override bool IsDone => Initialized && AssetHandle.IsDone;
            public override Exception Exception => AssetHandle.Exception;

            public override void Release() {
                AssetHandle.Release();
            }
        }

        /// <summary>
        /// ゲームオブジェクト情報
        /// </summary>
        private class GameObjectInfo : AssetInfo {
            public GameObject Key;
            
            public override bool IsDone => true;
            public override Exception Exception => null;

            public override void Release() {
            }
        }

        // UIService管理用
        private readonly Dictionary<Type, IUIService> _services = new();
        // シーン管理用
        private readonly Dictionary<string, SceneInfo> _sceneInfos = new();
        // プレファブ管理用
        private readonly Dictionary<string, PrefabInfo> _prefabInfos = new();
        // ゲームオブジェクト管理用
        private readonly Dictionary<GameObject, GameObjectInfo> _gameobjectInfos = new();

        // コルーチン制御
        private CoroutineRunner _coroutineRunner;
        // 読み込みに使うローダー
        private IUIAssetLoader _loader;
        // 時間管理用
        private LayeredTime _layeredTime;
        // Prefabインスタンス格納用Root
        private GameObject _rootObject;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            foreach (var info in _sceneInfos.Values.ToArray()) {
                RemoveAssetInfo(info);
            }

            foreach (var info in _prefabInfos.Values.ToArray()) {
                RemoveAssetInfo(info);
            }

            foreach (var info in _gameobjectInfos.Values.ToArray()) {
                RemoveAssetInfo(info);
            }

            _coroutineRunner.Dispose();

            if (_rootObject != null) {
                Object.Destroy(_rootObject);
                _rootObject = null;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;

            _coroutineRunner.Update();

            foreach (var service in _services.Values) {
                service.Update(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;
            foreach (var service in _services.Values) {
                service.LateUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize(IUIAssetLoader loader, LayeredTime layeredTime = null) {
            _loader = loader;
            _layeredTime = layeredTime;
            _coroutineRunner = new CoroutineRunner();
            _rootObject = new GameObject("UIManager_Root");
            Object.DontDestroyOnLoad(_rootObject);
        }

        /// <summary>
        /// UIが配置されたシーンの読み込み
        /// </summary>
        /// <param name="assetKey">読み込みに使うキー</param>
        public AssetHandle LoadSceneAsync(string assetKey) {
            if (_loader == null) {
                return new AssetHandle(new Exception($"Not found loader. [{assetKey}]"));
            }

            // 既に読み込みしている
            if (_sceneInfos.TryGetValue(assetKey, out var assetInfo)) {
                return new AssetHandle(this, assetInfo);
            }

            // 読み込み処理
            var handle = _loader.GetSceneAssetHandle(assetKey);
            assetInfo = new SceneInfo();
            assetInfo.Key = assetKey;
            assetInfo.AssetHandle = handle;
            _sceneInfos.Add(assetKey, assetInfo);

            IEnumerator Routine() {
                while (!handle.IsDone) {
                    yield return null;
                }

                if (handle.Exception != null) {
                    yield break;
                }

                yield return handle.ActivateAsync();

                var scene = handle.Scene;
                var rootCanvases = new List<Canvas>();
                foreach (var obj in scene.GetRootGameObjects()) {
                    var canvas = obj.GetComponent<Canvas>();
                    if (canvas != null) {
                        rootCanvases.Add(canvas);
                    }

                    var services = obj.GetComponentsInChildren<IUIService>();
                    foreach (var service in services) {
                        var serviceType = service.GetType();
                        if (_services.ContainsKey(serviceType)) {
                            Debug.LogWarning($"Already exists service type. [{serviceType}]");
                            continue;
                        }

                        assetInfo.ServiceTypes.Add(serviceType);
                        _services[serviceType] = service;
                        service.Initialize();
                    }
                }

                assetInfo.RootCanvases = rootCanvases.ToArray();
                assetInfo.Initialized = true;
            }

            // コルーチンの開始
            var coroutine = _coroutineRunner.StartCoroutine(Routine());
            assetInfo.Coroutine = coroutine;

            return new AssetHandle(this, assetInfo);
        }

        /// <summary>
        /// UIが配置されたプレファブの読み込み
        /// </summary>
        /// <param name="assetKey">読み込みに使うキー</param>
        public AssetHandle LoadPrefabAsync(string assetKey) {
            if (_loader == null) {
                return new AssetHandle(new Exception($"Not found loader. [{assetKey}]"));
            }

            // 既に読み込みしている
            if (_prefabInfos.TryGetValue(assetKey, out var assetInfo)) {
                return new AssetHandle(this, assetInfo);
            }

            // 読み込み処理
            var handle = _loader.GetPrefabAssetHandle(assetKey);
            assetInfo = new PrefabInfo();
            assetInfo.Key = assetKey;
            assetInfo.AssetHandle = handle;
            _prefabInfos.Add(assetKey, assetInfo);

            IEnumerator Routine() {
                while (!handle.IsDone) {
                    yield return null;
                }

                if (handle.Exception != null) {
                    yield break;
                }

                var prefab = handle.Asset;
                var instance = Object.Instantiate(prefab, _rootObject.transform, false);
                var services = instance.GetComponentsInChildren<IUIService>();
                foreach (var service in services) {
                    var serviceType = service.GetType();
                    if (_services.ContainsKey(serviceType)) {
                        Debug.LogWarning($"Already exists service type. [{serviceType}]");
                        continue;
                    }

                    assetInfo.ServiceTypes.Add(serviceType);
                    _services[serviceType] = service;
                    service.Initialize();
                }

                assetInfo.RootCanvases = instance.GetComponentsInChildren<Canvas>()
                    .Where(x => x.transform.parent is not RectTransform)
                    .ToArray();
                assetInfo.Initialized = true;
            }

            // コルーチンの開始
            var coroutine = _coroutineRunner.StartCoroutine(Routine());
            assetInfo.Coroutine = coroutine;

            return new AssetHandle(this, assetInfo);
        }

        /// <summary>
        /// ゲームオブジェクト単位でのUIServiceの追加
        /// </summary>
        public AssetHandle AddGameObject(GameObject gameObject) {
            // 既に登録済み
            if (_gameobjectInfos.TryGetValue(gameObject, out var assetInfo)) {
                return new AssetHandle(this, assetInfo);
            }
            
            // 登録処理
            assetInfo = new GameObjectInfo();
            assetInfo.Key = gameObject;
            _gameobjectInfos.Add(gameObject, assetInfo);
            
            // サービスの初期化
            var services = gameObject.GetComponentsInChildren<IUIService>();
            foreach (var service in services) {
                var serviceType = service.GetType();
                if (_services.ContainsKey(serviceType)) {
                    Debug.LogWarning($"Already exists service type. [{serviceType}]");
                    continue;
                }

                _services[serviceType] = service;
                service.Initialize();

                assetInfo.RootCanvases = gameObject.GetComponentsInChildren<Canvas>()
                    .Where(x => x.transform.parent is not RectTransform)
                    .ToArray();
                assetInfo.Initialized = true;
            }

            return new AssetHandle(this, assetInfo);
        }

        /// <summary>
        /// UIServiceの取得
        /// </summary>
        public T GetService<T>()
            where T : UIService {
            if (!_services.TryGetValue(typeof(T), out var service)) {
                return null;
            }

            return (T)service;
        }

        /// <summary>
        /// 現在存在するCanvasの一覧を取得
        /// </summary>
        public Canvas[] GetCanvases() {
            return _prefabInfos.SelectMany(x => x.Value.RootCanvases)
                .Concat(_sceneInfos.SelectMany(x => x.Value.RootCanvases))
                .Concat(_gameobjectInfos.SelectMany(x => x.Value.RootCanvases))
                .ToArray();
        }

        /// <summary>
        /// AssetInfoの削除
        /// </summary>
        private void RemoveAssetInfo(AssetInfo assetInfo) {
            if (assetInfo is SceneInfo sceneInfo) {
                if (!_sceneInfos.Remove(sceneInfo.Key)) {
                    return;
                }
            }
            else if (assetInfo is PrefabInfo prefabInfo) {
                if (!_prefabInfos.Remove(prefabInfo.Key)) {
                    return;
                }
            }
            else if (assetInfo is GameObjectInfo gameObjectInfo) {
                if (!_gameobjectInfos.Remove(gameObjectInfo.Key)) {
                    return;
                }
            }
            else {
                return;
            }

            if (assetInfo.Coroutine != null) {
                _coroutineRunner.StopCoroutine(assetInfo.Coroutine);
            }

            foreach (var serviceType in assetInfo.ServiceTypes) {
                _services[serviceType].Dispose();
                _services.Remove(serviceType);
            }

            assetInfo.Release();
        }
    }
}