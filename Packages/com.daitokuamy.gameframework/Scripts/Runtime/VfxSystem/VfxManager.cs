using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// Vfx管理クラス
    /// </summary>
    public class VfxManager : DisposableLateUpdatable {
        /// <summary>
        /// 再生管理用ハンドル
        /// </summary>
        public struct Handle : IDisposable, IProcess {
            private VfxManager _manager;
            private int _handleId;
            
            /// <inheritdoc/>
            object IEnumerator.Current => null;
            /// <inheritdoc/>
            bool IProcess.IsDone => !IsPlaying;
            /// <inheritdoc/>
            Exception IProcess.Exception => null;

            /// <summary>有効なハンドルか</summary>
            public bool IsValid => TryGetPlayingInfo(out var playingInfo) && playingInfo.IsHandleActive;
            /// <summary>再生中か</summary>
            public bool IsPlaying => TryGetPlayingInfo(out var playingInfo) && playingInfo.IsHandleActive && playingInfo.IsPlaying();
            /// <summary>廃棄済みか</summary>
            public bool IsDisposed => !TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsHandleActive;

            /// <summary>制御座標</summary>
            public Vector3 ContextPosition {
                get {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsHandleActive) {
                        return Vector3.zero;
                    }

                    return playingInfo.GetContextPosition();
                }
                set {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsHandleActive) {
                        return;
                    }

                    playingInfo.SetContextPosition(value);
                }
            }
            /// <summary>制御向き</summary>
            public Quaternion ContextRotation {
                get {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsHandleActive) {
                        return Quaternion.identity;
                    }

                    return playingInfo.GetContextRotation();
                }
                set {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsHandleActive) {
                        return;
                    }

                    playingInfo.SetContextRotation(value);
                }
            }
            /// <summary>制御スケール</summary>
            public Vector3 ContextLocalScale {
                get {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsHandleActive) {
                        return Vector3.zero;
                    }

                    return playingInfo.GetContextLocalScale();
                }
                set {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.IsHandleActive) {
                        return;
                    }

                    playingInfo.SetContextLocalScale(value);
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(VfxManager manager, int handleId) {
                _manager = manager;
                _handleId = handleId;
            }

            /// <inheritdoc/>
            public void Dispose() {
                if (IsDisposed) {
                    return;
                }

                _manager.DisposeHandle(_handleId);
                _manager = null;
                _handleId = 0;
            }

            /// <inheritdoc/>
            bool IEnumerator.MoveNext() {
                return IsPlaying;
            }

            /// <inheritdoc/>
            void IEnumerator.Reset() {
            }

            /// <summary>
            /// 再生
            /// </summary>
            public void Play() {
                if (IsDisposed) {
                    return;
                }

                _manager.PlayHandle(_handleId);
            }

            /// <summary>
            /// 停止
            /// </summary>
            public void Stop(bool immediate = false, bool autoDispose = false) {
                if (IsDisposed) {
                    return;
                }

                _manager.StopHandle(_handleId, immediate, autoDispose);
            }

            /// <summary>
            /// 再生情報の取得
            /// </summary>
            private bool TryGetPlayingInfo(out PlayingInfo playingInfo) {
                if (_manager == null) {
                    playingInfo = null;
                    return false;
                }

                return _manager.TryGetPlayingInfo(_handleId, out playingInfo);
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        internal class PlayingInfo {
            private Transform _positionRoot;
            private Transform _rotationRoot;
            private LayeredTime _layeredTime;
            private ILodProvider _lodProvider;
            private VfxContext _context;
            private bool _autoDispose;
            private bool _transformDirty;

            /// <summary>制御対象Object情報</summary>
            public ObjectInfo ObjectInfo { get; private set; }
            /// <summary>ハンドル識別子</summary>
            public int HandleId { get; private set; }
            /// <summary>廃棄済みか</summary>
            public bool Initialized { get; private set; }
            /// <summary>ハンドルが有効か</summary>
            public bool IsHandleActive => Initialized && (!_autoDispose || IsPlaying());

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Setup(ObjectInfo objectInfo, VfxContext context, Transform positionRoot, Transform rotationRoot, LayeredTime layeredTime, ILodProvider lodProvider, int layer,
                int handleId, bool autoDispose) {
                Cleanup();

                ObjectInfo = objectInfo;
                HandleId = handleId;
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
            /// Pool返却前の後始末
            /// </summary>
            public ObjectInfo ReleaseObjectInfo() {
                var objectInfo = ObjectInfo;

                ObjectInfo = null;
                HandleId = 0;
                _positionRoot = null;
                _rotationRoot = null;
                _layeredTime = null;
                _lodProvider = null;
                _context = default;
                _autoDispose = false;
                _transformDirty = false;

                return objectInfo;
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

                var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Mathf.Max(Time.deltaTime, 1.0f / 120.0f);
                for (var i = 0; i < ObjectInfo.Components.Length; i++) {
                    var component = ObjectInfo.Components[i];
                    if (!component.IsPlaying) {
                        continue;
                    }

                    component.Tick(deltaTime);
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

        private readonly Transform _rootTransform;
        private readonly KeyedObjectPool<GameObject, ObjectInfo> _objectPool;
        private readonly InstancePool<PlayingInfo> _playingInfoPool;
        private readonly List<PlayingInfo> _playingInfos = new();
        private readonly Dictionary<int, PlayingInfo> _playingInfoMap = new();
        private readonly List<ParticleSystem> _workParticleSystems = new();
        private bool _activePool = true;
        private int _nextHandleId = 1;

        /// <summary>デフォルト指定のLayer</summary>
        public int DefaultLayer { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="poolDefaultCapacity">Poolのデフォルトキャパシティ</param>
        /// <param name="poolMaxCapacity">Poolの最大キャパシティ</param>
        public VfxManager(int poolDefaultCapacity = 10, int poolMaxCapacity = 10000) {
            var root = new GameObject(nameof(VfxManager), typeof(VfxManagerDispatcher));
            var dispatcher = root.GetComponent<VfxManagerDispatcher>();
            dispatcher.Setup(this);
            Object.DontDestroyOnLoad(root);
            _rootTransform = root.transform;

            _playingInfoPool = new InstancePool<PlayingInfo>();

            // Pool生成
            _objectPool = new KeyedObjectPool<GameObject, ObjectInfo>(prefab => {
                var pool = new ObjectPool<ObjectInfo, GameObject>(prefab, pfb => {
                        var objectInfo = new ObjectInfo();
                        objectInfo.Prefab = pfb;

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

                        return objectInfo;
                    }, (_, info) => {
                        info.Root.SetActive(true);
                    }, (_, info) => {
                        info.Root.SetActive(false);
                    },
                    (_, info) => {
                        Object.Destroy(info.Root);
                        info.Root = null;
                        info.Components = null;
                    }, true, poolDefaultCapacity, poolMaxCapacity);

                return pool;
            });
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            Clear();

            _playingInfoPool.Dispose();
            if (_rootTransform != null) {
                Object.Destroy(_rootTransform.gameObject);
            }
        }

        /// <inheritdoc/>
        protected override void LateUpdateInternal() {
            // 再生中情報の更新
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];

                // 更新処理
                info.Update();

                // 廃棄対象ならPoolに戻す
                if (!info.Initialized) {
                    _playingInfos.RemoveAt(i);
                    ReleasePlayingInfo(info);
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

            _activePool = active;
            
            Clear();
            _objectPool.SetPoolingEnabled(_activePool, true);
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
            return playingInfo != null ? new Handle(this, playingInfo.HandleId) : default;
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
            return playingInfo != null ? new Handle(this, playingInfo.HandleId) : default;
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
                ReleasePlayingInfo(info);
            }

            // Poolを全部削除
            _objectPool.ClearAll();
            _playingInfoPool.Clear();
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
            var handleId = _nextHandleId++;
            var playingInfo = _playingInfoPool.Get();
            playingInfo.Setup(objectInfo, context, positionRoot, rotationRoot, layeredTime, lodProvider, layer, handleId, autoDispose);
            playingInfo.Stop(true, false);
            _playingInfos.Add(playingInfo);
            _playingInfoMap.Add(handleId, playingInfo);

            return playingInfo;
        }

        /// <summary>
        /// 再生情報の取得
        /// </summary>
        private bool TryGetPlayingInfo(int handleId, out PlayingInfo playingInfo) {
            if (handleId <= 0) {
                playingInfo = null;
                return false;
            }

            return _playingInfoMap.TryGetValue(handleId, out playingInfo);
        }

        /// <summary>
        /// Handle経由の廃棄
        /// </summary>
        private void DisposeHandle(int handleId) {
            if (!_playingInfoMap.TryGetValue(handleId, out var playingInfo) || !playingInfo.IsHandleActive) {
                return;
            }

            playingInfo.Cleanup();
        }

        /// <summary>
        /// Handle経由の再生
        /// </summary>
        private void PlayHandle(int handleId) {
            if (!_playingInfoMap.TryGetValue(handleId, out var playingInfo) || !playingInfo.IsHandleActive) {
                return;
            }

            playingInfo.Play();
        }

        /// <summary>
        /// Handle経由の停止
        /// </summary>
        private void StopHandle(int handleId, bool immediate, bool autoDispose) {
            if (!_playingInfoMap.TryGetValue(handleId, out var playingInfo) || !playingInfo.IsHandleActive) {
                return;
            }

            playingInfo.Stop(immediate, autoDispose);
        }

        /// <summary>
        /// PlayingInfoの返却
        /// </summary>
        private void ReleasePlayingInfo(PlayingInfo playingInfo) {
            if (playingInfo == null) {
                return;
            }

            _playingInfoMap.Remove(playingInfo.HandleId);

            var objectInfo = playingInfo.ReleaseObjectInfo();
            _playingInfoPool.Release(playingInfo);
            ReturnObjectInfo(objectInfo);
        }

        /// <summary>
        /// ObjectInfoの取得
        /// </summary>
        private ObjectInfo GetObjectInfo(GameObject prefab) {
            if (prefab == null) {
                Debug.unityLogger.LogError(nameof(VfxManager), "prefab is null.");
                return null;
            }

            return _objectPool.Get(prefab);
        }

        /// <summary>
        /// ObjectInfoの返却
        /// </summary>
        private void ReturnObjectInfo(ObjectInfo objectInfo) {
            if (objectInfo == null || objectInfo.Prefab == null) {
                return;
            }

            _objectPool.Release(objectInfo.Prefab, objectInfo);
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
