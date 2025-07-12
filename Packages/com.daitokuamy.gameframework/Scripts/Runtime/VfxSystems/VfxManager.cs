using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// Vfx管理クラス
    /// </summary>
    public class VfxManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 再生管理用ハンドル
        /// </summary>
        public struct Handle : IDisposable, IProcess {
            private PlayingInfo _playingInfo;
            /// <summary>未使用</summary>
            object IEnumerator.Current => null;
            /// <summary>完了しているか</summary>
            bool IProcess.IsDone => !IsPlaying;
            /// <summary>エラー</summary>
            Exception IProcess.Exception => null;

            /// <summary>有効なハンドルか</summary>
            public bool IsValid => !IsDisposed && _playingInfo.Initialized;
            /// <summary>再生中か</summary>
            public bool IsPlaying => !IsDisposed && _playingInfo.IsPlaying();
            /// <summary>廃棄済みか</summary>
            public bool IsDisposed => _playingInfo == null;

            /// <summary>制御座標</summary>
            public Vector3 ContextPosition {
                get {
                    if (_playingInfo == null) {
                        return Vector3.zero;
                    }

                    return _playingInfo.GetContextPosition();
                }
                set {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.SetContextPosition(value);
                }
            }
            /// <summary>制御向き</summary>
            public Quaternion ContextRotation {
                get {
                    if (_playingInfo == null) {
                        return Quaternion.identity;
                    }

                    return _playingInfo.GetContextRotation();
                }
                set {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.SetContextRotation(value);
                }
            }
            /// <summary>制御スケール</summary>
            public Vector3 ContextLocalScale {
                get {
                    if (_playingInfo == null) {
                        return Vector3.zero;
                    }

                    return _playingInfo.GetContextLocalScale();
                }
                set {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.SetContextLocalScale(value);
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo info) {
                _playingInfo = info;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (IsDisposed) {
                    return;
                }

                _playingInfo.Cleanup();
                _playingInfo = null;
            }

            /// <summary>
            /// 継続実行するか
            /// </summary>
            bool IEnumerator.MoveNext() {
                return IsPlaying;
            }

            /// <summary>
            /// 未使用
            /// </summary>
            void IEnumerator.Reset() {
            }

            /// <summary>
            /// 再生
            /// </summary>
            public void Play() {
                if (IsDisposed) {
                    return;
                }

                _playingInfo.Play();
            }

            /// <summary>
            /// 停止
            /// </summary>
            public void Stop(bool immediate = false, bool autoDispose = false) {
                if (IsDisposed) {
                    return;
                }

                _playingInfo.Stop(immediate, autoDispose);
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        internal class PlayingInfo {
            // 追従基準にするTransform
            private Transform _positionRoot;
            private Transform _rotationRoot;
            // TimeScale変更用LayeredTime
            private LayeredTime _layeredTime;
            // Lodレベル通知用インターフェース
            private ILodProvider _lodProvider;
            // 操作用の情報
            private VfxContext _context;
            // 自動廃棄するか
            private bool _autoDispose;
            // Transform更新フラグ
            private bool _transformDirty;

            /// <summary>制御対象Object情報</summary>
            public ObjectInfo ObjectInfo { get; private set; }
            /// <summary>廃棄済みか</summary>
            public bool Initialized { get; private set; }

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Setup(ObjectInfo objectInfo, VfxContext context, Transform positionRoot, Transform rotationRoot, LayeredTime layeredTime, ILodProvider lodProvider, int layer, bool autoDispose) {
                Cleanup();

                ObjectInfo = objectInfo;
                _context = context;
                _positionRoot = positionRoot;
                _rotationRoot = rotationRoot;
                _layeredTime = layeredTime;
                _lodProvider = lodProvider;
                _autoDispose = autoDispose;
                _transformDirty = true;

                void SetLayer(Transform trans, int value) {
                    if (trans == null) {
                        return;
                    }

                    trans.gameObject.layer = value;
                    
                    for (var i = 0; i < trans.childCount; i++) {
                        SetLayer(trans.GetChild(i), value);
                    }
                }

                // 再帰的にレイヤー設定
                SetLayer(objectInfo.Root.transform, layer);

                Initialized = true;

                if (layeredTime != null) {
                    layeredTime.ChangedTimeScaleEvent += OnChangedTimeScale;
                }

                if (lodProvider != null) {
                    lodProvider.ChangedLodLevelEvent += OnChangedLodLevel;
                }

                OnChangedTimeScale(layeredTime?.TimeScale ?? 1.0f);
                OnChangedLodLevel(lodProvider?.LodLevel ?? 0);
            }

            /// <summary>
            /// 解放処理
            /// </summary>
            public void Cleanup() {
                if (!Initialized) {
                    return;
                }

                Stop(true, true);

                if (_layeredTime != null) {
                    _layeredTime.ChangedTimeScaleEvent -= OnChangedTimeScale;
                }

                if (_lodProvider != null) {
                    _lodProvider.ChangedLodLevelEvent -= OnChangedLodLevel;
                }

                OnChangedTimeScale(1.0f);
                OnChangedLodLevel(0);

                Initialized = false;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public void Update() {
                if (!Initialized) {
                    return;
                }

                var dirty = _transformDirty;
                _transformDirty = false;

                var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;
                for (var i = 0; i < ObjectInfo.Components.Length; i++) {
                    var component = ObjectInfo.Components[i];
                    if (!component.IsPlaying) {
                        continue;
                    }

                    component.Update(deltaTime);
                }

                if (dirty || _context.constraintPosition) {
                    UpdatePosition();
                }

                if (dirty || _context.constraintRotation) {
                    UpdateRotation();
                }

                if (dirty) {
                    UpdateScale();
                }

                // 自動廃棄処理
                if (_autoDispose && !IsPlaying()) {
                    Cleanup();
                }
            }

            /// <summary>
            /// 再生処理
            /// </summary>
            public void Play() {
                if (!Initialized) {
                    return;
                }

                // Transform更新
                UpdatePosition();
                UpdateRotation();
                UpdateScale();
                _transformDirty = false;

                for (var i = 0; i < ObjectInfo.Components.Length; i++) {
                    var component = ObjectInfo.Components[i];
                    component.Play();
                }
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop(bool immediate, bool autoDispose) {
                if (!Initialized) {
                    return;
                }

                // 停止時にAutoDisposeが指定されたら上書きする
                _autoDispose |= autoDispose;

                for (var i = 0; i < ObjectInfo.Components.Length; i++) {
                    var component = ObjectInfo.Components[i];
                    if (immediate) {
                        component.StopImmediate();
                    }
                    else {
                        component.Stop();
                    }
                }
            }

            /// <summary>
            /// 座標の取得
            /// </summary>
            public Vector3 GetContextPosition() {
                if (!Initialized) {
                    return Vector3.zero;
                }

                return _context.relativePosition;
            }

            /// <summary>
            /// 向きの取得
            /// </summary>
            public Quaternion GetContextRotation() {
                if (!Initialized) {
                    return Quaternion.identity;
                }

                return Quaternion.Euler(_context.relativeAngles);
            }

            /// <summary>
            /// スケールの取得
            /// </summary>
            public Vector3 GetContextLocalScale() {
                if (!Initialized) {
                    return Vector3.one;
                }

                return _context.localScale;
            }

            /// <summary>
            /// 座標の設定
            /// </summary>
            public void SetContextPosition(Vector3 position) {
                if (!Initialized) {
                    return;
                }

                _context.relativePosition = position;
                _transformDirty = true;
            }

            /// <summary>
            /// 座標の設定
            /// </summary>
            public void SetContextRotation(Quaternion rotation) {
                if (!Initialized) {
                    return;
                }

                _context.relativeAngles = rotation.eulerAngles;
                _transformDirty = true;
            }

            /// <summary>
            /// スケールの設定
            /// </summary>
            public void SetContextLocalScale(Vector3 localScale) {
                if (!Initialized) {
                    return;
                }

                _context.localScale = localScale;
                _transformDirty = true;
            }

            /// <summary>
            /// 再生中か
            /// </summary>
            public bool IsPlaying() {
                if (!Initialized) {
                    return false;
                }

                for (var i = 0; i < ObjectInfo.Components.Length; i++) {
                    var component = ObjectInfo.Components[i];
                    if (component.IsPlaying) {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 座標の更新
            /// </summary>
            private void UpdatePosition() {
                var rootTrans = ObjectInfo.Root.transform;
                if (_positionRoot != null) {
                    rootTrans.position = _positionRoot.TransformPoint(_context.relativePosition);
                }
                else {
                    rootTrans.position = _context.relativePosition;
                }
            }

            /// <summary>
            /// 回転の更新
            /// </summary>
            private void UpdateRotation() {
                var rootTrans = ObjectInfo.Root.transform;
                if (_rotationRoot != null) {
                    rootTrans.rotation = _rotationRoot.rotation * Quaternion.Euler(_context.relativeAngles);
                }
                else {
                    rootTrans.rotation = Quaternion.Euler(_context.relativeAngles);
                }
            }

            /// <summary>
            /// 拡縮の更新
            /// </summary>
            private void UpdateScale() {
                var rootTrans = ObjectInfo.Root.transform;
                rootTrans.localScale = _context.localScale;
            }

            /// <summary>
            /// TimeScaleの変更通知
            /// </summary>
            private void OnChangedTimeScale(float timeScale) {
                if (!Initialized) {
                    return;
                }

                for (var i = 0; i < ObjectInfo.Components.Length; i++) {
                    var component = ObjectInfo.Components[i];
                    component.SetSpeed(timeScale);
                }
            }

            /// <summary>
            /// LodLevelの変更通知
            /// </summary>
            private void OnChangedLodLevel(int level) {
                if (!Initialized) {
                    return;
                }

                for (var i = 0; i < ObjectInfo.Components.Length; i++) {
                    var component = ObjectInfo.Components[i];
                    component.SetLodLevel(level);
                }
            }
        }

        /// <summary>
        /// プール用Objectの情報
        /// </summary>
        public class ObjectInfo {
            public GameObject Prefab;
            public GameObject Root;
            public IVfxComponent[] Components;
        }

        // Poolキャパシティ
        private readonly int _poolDefaultCapacity;
        private readonly int _poolMaxCapacity;

        // 生成したGameObjectを保持するためのTransform
        private readonly Transform _rootTransform;
        // インスタンスキャッシュ用のPool
        private readonly Dictionary<GameObject, ObjectPool<ObjectInfo>> _objectPools = new();
        // PlayingInfoインスタンス使いまわし用のPool
        private readonly ObjectPool<PlayingInfo> _playingInfoPool;
        // 管理用再生中情報
        private readonly List<PlayingInfo> _playingInfos = new();
        // 変数領域確保用のParticleSystemリスト
        private readonly List<ParticleSystem> _workParticleSystems = new();

        // Poolを有効にするフラグ
        private bool _activePool = true;

        /// <summary>デフォルト指定のLayer</summary>
        public int DefaultLayer { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="poolDefaultCapacity">Poolのデフォルトキャパシティ</param>
        /// <param name="poolMaxCapacity">Poolの最大キャパシティ</param>
        public VfxManager(int poolDefaultCapacity = 10, int poolMaxCapacity = 10000) {
            _poolDefaultCapacity = poolDefaultCapacity;
            _poolMaxCapacity = poolMaxCapacity;

            var root = new GameObject(nameof(VfxManager), typeof(VfxManagerDispatcher));
            var dispatcher = root.GetComponent<VfxManagerDispatcher>();
            dispatcher.Setup(this);
            Object.DontDestroyOnLoad(root);
            _rootTransform = root.transform;

            _playingInfoPool = new ObjectPool<PlayingInfo>(() => new PlayingInfo());
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            Clear();

            _playingInfoPool.Dispose();
            if (_rootTransform != null) {
                Object.Destroy(_rootTransform.gameObject);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            // 再生中情報の更新
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];

                // 更新処理
                info.Update();

                // 廃棄対象ならPoolに戻す
                if (!info.Initialized) {
                    _playingInfos.RemoveAt(i);
                    _playingInfoPool.Release(info);
                    ReturnObjectInfo(info.ObjectInfo);
                }
            }
        }

        /// <summary>
        /// Poolの有効状態を変更(Debug用)
        /// </summary>
        public void SetActivePool(bool active) {
            if (active == _activePool) {
                return;
            }

            Clear();
            _activePool = active;
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="context">再生に必要なコンテキスト</param>
        /// <param name="positionRoot">座標決定の基準にするTransform</param>
        /// <param name="rotationRoot">回転決定の基準にするTransform</param>
        /// <param name="layeredTime">再生速度をコントロールするためのLayeredTime</param>
        /// <param name="lodProvider">Lodレベル提供用インターフェース</param>
        /// <param name="layer">指定するLayer</param>
        public Handle Get(VfxContext context, Transform positionRoot = null, Transform rotationRoot = null, LayeredTime layeredTime = null, ILodProvider lodProvider = null, int layer = -1) {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(context, positionRoot, rotationRoot, layeredTime, lodProvider, layer, false);
            // Handle化して返却
            return new Handle(playingInfo);
        }

        /// <summary>
        /// 再生(再生完了時に自動的にHandleがDisposeされる)
        /// </summary>
        /// <param name="context">再生に必要なコンテキスト</param>
        /// <param name="positionRoot">座標決定の基準にするTransform</param>
        /// <param name="rotationRoot">回転決定の基準にするTransform</param>
        /// <param name="layeredTime">再生速度をコントロールするためのLayeredTime</param>
        /// <param name="lodProvider">Lodレベル提供用インターフェース</param>
        /// <param name="layer">指定するLayer</param>
        public Handle Play(VfxContext context, Transform positionRoot = null, Transform rotationRoot = null, LayeredTime layeredTime = null, ILodProvider lodProvider = null, int layer = -1) {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(context, positionRoot, rotationRoot, layeredTime, lodProvider, layer, true);
            // 再生
            playingInfo?.Play();
            // Handle化して返却
            return new Handle(playingInfo);
        }

        /// <summary>
        /// 再生しているエフェクトとPoolの状態をクリア
        /// </summary>
        public void Clear() {
            // Poolに全部戻して削除
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];

                // 廃棄
                info.Cleanup();

                // Poolに戻す
                _playingInfos.RemoveAt(i);
                _playingInfoPool.Release(info);
                ReturnObjectInfo(info.ObjectInfo);
            }

            // Poolを全部削除
            foreach (var pool in _objectPools.Values) {
                pool.Dispose();
            }

            _playingInfoPool.Clear();
            _objectPools.Clear();
        }

        /// <summary>
        /// 再生情報の生成
        /// </summary>
        /// <param name="context">再生に必要なコンテキスト</param>
        /// <param name="positionRoot">座標決定の基準にするTransform</param>
        /// <param name="rotationRoot">回転決定の基準にするTransform</param>
        /// <param name="layeredTime">再生速度をコントロールするためのLayeredTime</param>
        /// <param name="lodProvider">Lodレベル提供用インターフェース</param>
        /// <param name="autoDispose">再生完了時に自動で廃棄するか</param>
        /// <param name="layer">レイヤー</param>
        private PlayingInfo CreatePlayingInfo(VfxContext context, Transform positionRoot, Transform rotationRoot, LayeredTime layeredTime, ILodProvider lodProvider, int layer, bool autoDispose) {
            // Instance生成
            var objectInfo = GetObjectInfo(context.prefab);
            if (objectInfo == null) {
                return null;
            }

            // 未指定のLayerの場合はDefault値を使用
            if (layer < 0) {
                layer = DefaultLayer;
            }

            // 再生情報の構築
            var playingInfo = _playingInfoPool.Get();
            playingInfo.Setup(objectInfo, context, positionRoot, rotationRoot, layeredTime, lodProvider, layer, autoDispose);
            playingInfo.Stop(true, false);
            _playingInfos.Add(playingInfo);

            return playingInfo;
        }

        /// <summary>
        /// ObjectInfoの取得
        /// </summary>
        private ObjectInfo GetObjectInfo(GameObject prefab) {
            if (prefab == null) {
                Debug.unityLogger.LogError(nameof(VfxManager), "prefab is null.");
                return null;
            }

            // Poolが作られていなければ、ここで生成
            if (!_objectPools.TryGetValue(prefab, out var pool)) {
                pool = CreatePool(prefab, _activePool);
                _objectPools[prefab] = pool;
            }

            return pool.Get();
        }

        /// <summary>
        /// ObjectInfoの返却
        /// </summary>
        private void ReturnObjectInfo(ObjectInfo objectInfo) {
            if (objectInfo.Prefab == null) {
                return;
            }

            if (!_objectPools.TryGetValue(objectInfo.Prefab, out var pool)) {
                Debug.unityLogger.LogWarning(nameof(VfxManager), $"Not found object pool. {objectInfo.Prefab.name}");
                return;
            }

            pool.Release(objectInfo);
        }

        /// <summary>
        /// Poolの生成
        /// </summary>
        /// <param name="activePool">InstanceをPoolするか</param>
        private ObjectPool<ObjectInfo> CreatePool(GameObject prefab, bool activePool) {
            // 中身の生成
            void CreateContent(ObjectInfo objectInfo) {
                if (objectInfo == null || objectInfo.Prefab == null) {
                    return;
                }

                var instance = Object.Instantiate(objectInfo.Prefab, _rootTransform);
                var foundComponents = instance.GetComponentsInChildren<IVfxComponent>(true);
                _workParticleSystems.Clear();
                FindRootParticleSystems(instance.transform, _workParticleSystems);
                var vfxComponents = new List<IVfxComponent>(foundComponents.Length + _workParticleSystems.Count);
                for (var i = 0; i < foundComponents.Length; ++i) {
                    vfxComponents.Add(foundComponents[i]);
                }

                for (var i = 0; i < _workParticleSystems.Count; ++i) {
                    vfxComponents.Add(new ParticleSystemVfxComponent(_workParticleSystems[i]));
                }

                instance.SetActive(false);

                // Componentを一度停止状態にしておく
                foreach (var component in vfxComponents) {
                    component.StopImmediate();
                }

                objectInfo.Root = instance;
                objectInfo.Components = vfxComponents.ToArray();
            }

            // 中身の削除
            void DestroyContent(ObjectInfo objectInfo) {
                if (objectInfo == null || objectInfo.Root == null) {
                    return;
                }

                Object.Destroy(objectInfo.Root);
                objectInfo.Root = null;
                objectInfo.Components = null;
            }

            var pool = new ObjectPool<ObjectInfo>(() => {
                    var objectInfo = new ObjectInfo();
                    objectInfo.Prefab = prefab;

                    if (activePool) {
                        CreateContent(objectInfo);
                    }

                    return objectInfo;
                }, info => {
                    if (activePool) {
                        info.Root.SetActive(true);
                    }
                    else {
                        CreateContent(info);
                        info.Root.SetActive(true);
                    }
                }, info => {
                    if (activePool) {
                        info.Root.SetActive(false);
                    }
                    else {
                        info.Root.SetActive(false);
                        DestroyContent(info);
                    }
                },
                DestroyContent, true, _poolDefaultCapacity, _poolMaxCapacity);

            return pool;
        }

        /// <summary>
        /// RootになりえるParticleSystemのリストを階層的に構築
        /// </summary>
        private void FindRootParticleSystems(Transform parent, List<ParticleSystem> foundParticleSystems) {
            var ps = parent.GetComponent<ParticleSystem>();
            if (ps != null) {
                foundParticleSystems.Add(ps);
                return;
            }

            for (var i = 0; i < parent.childCount; i++) {
                FindRootParticleSystems(parent.GetChild(i), foundParticleSystems);
            }
        }
    }
}