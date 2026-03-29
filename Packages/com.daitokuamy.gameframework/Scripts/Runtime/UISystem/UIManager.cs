using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.AssetSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.UISystem {
    /// <summary>
    /// Uiの管理クラス
    /// </summary>
    public class UIManager : DisposableUpdateAndLateUpdatable {
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
                if (_assetInfo != null && _uIManager != null) {
                    _uIManager.RemoveAssetInfo(_assetInfo);
                    _assetInfo = null;
                    _uIManager = null;
                }
            }

            /// <inheritdoc/>
            bool IEnumerator.MoveNext() {
                return !IsDone;
            }

            /// <inheritdoc/>
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
            public Canvas[] RootCanvases = Array.Empty<Canvas>();

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
            public IUIAssetLoader Loader;
            public ISceneProcess Process;

            public override bool IsDone => Initialized && Process.IsDone;
            public override Exception Exception => Process.Exception;

            public override void Release() {
                Loader?.UnloadScene(Key);
            }
        }

        /// <summary>
        /// プレファブ情報
        /// </summary>
        private class PrefabInfo : AssetInfo {
            public string Key;
            public IUIAssetLoader Loader;
            public IProcess<GameObject> Process;
            public GameObject Instance;

            public override bool IsDone => Initialized && Process.IsDone;
            public override Exception Exception => Process.Exception;

            public override void Release() {
                if (Instance != null) {
                    Object.Destroy(Instance);
                    Instance = null;
                }

                Loader?.UnloadPrefab(Key);
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void UpdateInternal() {
            var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;

            _coroutineRunner.Update();

            foreach (var service in _services.Values) {
                service.Update(deltaTime);
            }
        }

        /// <inheritdoc/>
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
            var process = _loader.LoadSceneAsync(assetKey);
            assetInfo = new SceneInfo();
            assetInfo.Key = assetKey;
            assetInfo.Loader = _loader;
            assetInfo.Process = process;
            _sceneInfos.Add(assetKey, assetInfo);

            IEnumerator Routine() {
                while (!process.IsDone) {
                    yield return null;
                }

                if (process.Exception != null) {
                    assetInfo.Initialized = true;
                    yield break;
                }

                yield return process.ActivateAsync();

                var scene = process.Scene;
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
            var process = _loader.LoadPrefabAsync(assetKey);
            assetInfo = new PrefabInfo();
            assetInfo.Key = assetKey;
            assetInfo.Loader = _loader;
            assetInfo.Process = process;
            _prefabInfos.Add(assetKey, assetInfo);

            IEnumerator Routine() {
                while (!process.IsDone) {
                    yield return null;
                }

                if (process.Exception != null) {
                    assetInfo.Initialized = true;
                    yield break;
                }

                var prefab = process.Result;
                var instance = Object.Instantiate(prefab, _rootObject.transform, false);
                assetInfo.Instance = instance;
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

                assetInfo.ServiceTypes.Add(serviceType);
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
