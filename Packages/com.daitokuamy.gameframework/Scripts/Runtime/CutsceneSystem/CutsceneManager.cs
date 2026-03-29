using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Pooling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.CutsceneSystem {
    /// <summary>
    /// カットシーン管理クラス
    /// </summary>
    public sealed class CutsceneManager : DisposableLateUpdatable {
        /// <summary>
        /// 再生管理用ハンドル
        /// </summary>
        public struct Handle : IDisposable, IEventProcess {
            private CutsceneManager _manager;
            private int _handleId;

            /// <summary>再生中か</summary>
            public bool IsPlaying => TryGetPlayingInfo(out var playingInfo) && playingInfo.Initialized && playingInfo.IsPlaying();

            /// <summary>完了しているか</summary>
            public bool IsDone => !IsPlaying;

            /// <summary>未使用</summary>
            object IEnumerator.Current => null;

            /// <summary>エラー</summary>
            Exception IProcess.Exception => null;

            /// <summary>終了通知</summary>
            event Action IEventProcess.ExitEvent {
                add {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.Initialized) {
                        return;
                    }

                    playingInfo.OneshotStopEvent += value;
                }
                remove {
                    if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.Initialized) {
                        return;
                    }

                    playingInfo.OneshotStopEvent -= value;
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(CutsceneManager manager, int handleId) {
                _manager = manager;
                _handleId = handleId;
            }

            /// <inheritdoc/>
            public EventProcessAwaiter GetAwaiter() {
                return new EventProcessAwaiter(this);
            }

            /// <summary>
            /// 再生
            /// </summary>
            public void Play<T>(Action<T> onSetup)
                where T : ICutscene {
                if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.Initialized) {
                    return;
                }

                playingInfo.Play(onSetup);
            }

            /// <summary>
            /// 再生
            /// </summary>
            public void Play() {
                if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.Initialized) {
                    return;
                }

                playingInfo.Play<ICutscene>(null);
            }

            /// <summary>
            /// 停止
            /// </summary>
            public void Stop(bool autoDispose = false) {
                if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.Initialized) {
                    return;
                }

                playingInfo.Stop(autoDispose);
            }

            /// <summary>
            /// 時間の設定
            /// </summary>
            public void SetTime(float time) {
                if (!TryGetPlayingInfo(out var playingInfo) || !playingInfo.Initialized) {
                    return;
                }

                playingInfo.SetTime(time);
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (_manager != null) {
                    _manager.DisposeHandle(_handleId);
                }

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
            /// 再生中情報の取得
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
            private LayeredTime _layeredTime;
            private bool _autoDispose;
            private bool _pendingRelease;
            private bool _hasPendingStartTime;
            private float _pendingStartTime;

            /// <summary>再生通知</summary>
            public event Action PlayEvent;

            /// <summary>停止通知</summary>
            public event Action StopEvent;

            /// <summary>停止通知(1回)</summary>
            public event Action OneshotStopEvent;

            /// <summary>ハンドル識別子</summary>
            public int HandleId { get; private set; }

            /// <summary>初期化済みか</summary>
            public bool Initialized { get; private set; }

            /// <summary>制御対象</summary>
            public CutsceneInfo CutsceneInfo { get; private set; }

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Setup(CutsceneInfo cutsceneInfo, LayeredTime layeredTime, int handleId, bool autoDispose) {
                Cleanup();

                Initialized = true;
                HandleId = handleId;
                CutsceneInfo = cutsceneInfo;
                _layeredTime = layeredTime;
                _autoDispose = autoDispose;
                _pendingRelease = false;
                _hasPendingStartTime = false;
                _pendingStartTime = 0.0f;

                if (layeredTime != null) {
                    layeredTime.ChangedTimeScaleEvent += OnChangedTimeScale;
                    OnChangedTimeScale(layeredTime.TimeScale);
                }
                else {
                    OnChangedTimeScale(1.0f);
                }
            }

            /// <summary>
            /// クリーン処理
            /// </summary>
            public void Cleanup() {
                if (!Initialized) {
                    return;
                }

                if (IsCutsceneAlive()) {
                    Stop(true);
                }

                if (_layeredTime != null) {
                    _layeredTime.ChangedTimeScaleEvent -= OnChangedTimeScale;
                }

                Initialized = false;
            }

            /// <summary>返却待ちか</summary>
            public bool PendingRelease => _pendingRelease;

            /// <summary>
            /// 即時返却待ちフラグを立てる
            /// </summary>
            public void MarkPendingRelease() {
                _pendingRelease = true;
            }

            /// <summary>
            /// Pool返却前の後始末
            /// </summary>
            public CutsceneInfo ReleaseCutsceneInfo() {
                var cutsceneInfo = CutsceneInfo;

                HandleId = 0;
                CutsceneInfo = null;
                _layeredTime = null;
                _autoDispose = false;
                _pendingRelease = false;
                _hasPendingStartTime = false;
                _pendingStartTime = 0.0f;
                PlayEvent = null;
                StopEvent = null;
                OneshotStopEvent = null;

                return cutsceneInfo;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Update() {
                if (!Initialized) {
                    return false;
                }

                if (!IsCutsceneAlive()) {
                    Cleanup();
                    return false;
                }

                var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;
                var playing = CutsceneInfo.Cutscene?.IsPlaying ?? false;
                if (playing) {
                    // Cutsceneの更新
                    CutsceneInfo.Cutscene.Update(deltaTime);

                    // 再生停止
                    playing = CutsceneInfo.Cutscene.IsPlaying;
                    if (!playing) {
                        CutsceneInfo.Cutscene.Stop();
                        StopEvent?.Invoke();
                        OneshotStopEvent?.Invoke();
                        OneshotStopEvent = null;
                    }
                }

                // 自動廃棄処理
                if (_autoDispose && !playing) {
                    Cleanup();
                }

                // Cleanupされていなければ true
                return Initialized;
            }

            /// <summary>
            /// 再生処理
            /// </summary>
            public void Play<T>(Action<T> onSetup)
                where T : ICutscene {
                if (!Initialized) {
                    return;
                }

                if (!IsCutsceneAlive()) {
                    return;
                }

                if (CutsceneInfo.Cutscene.IsPlaying) {
                    return;
                }

                CutsceneInfo.Root.SetActive(true);

                if (onSetup != null && CutsceneInfo.Cutscene is T cutscene) {
                    onSetup.Invoke(cutscene);
                }

                CutsceneInfo.Cutscene.Seek(_hasPendingStartTime ? _pendingStartTime : 0.0f);
                _hasPendingStartTime = false;
                CutsceneInfo.Cutscene.Play();

                PlayEvent?.Invoke();
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop(bool autoDispose) {
                if (!Initialized) {
                    return;
                }

                // 停止時にAutoDisposeが指定されたら上書きする
                _autoDispose |= autoDispose;

                if (!IsCutsceneAlive()) {
                    return;
                }

                if (!CutsceneInfo.Cutscene.IsPlaying) {
                    return;
                }

                CutsceneInfo.Cutscene.Stop();
                StopEvent?.Invoke();
                OneshotStopEvent?.Invoke();
                OneshotStopEvent = null;
            }

            /// <summary>
            /// 時間の設定
            /// </summary>
            public void SetTime(float time) {
                if (!Initialized) {
                    return;
                }

                if (!IsCutsceneAlive()) {
                    return;
                }

                if (!CutsceneInfo.Cutscene.IsPlaying) {
                    _pendingStartTime = time;
                    _hasPendingStartTime = true;
                }

                CutsceneInfo.Cutscene.Seek(time);
            }

            /// <summary>
            /// 再生中か
            /// </summary>
            public bool IsPlaying() {
                if (!Initialized) {
                    return false;
                }

                if (IsCutsceneAlive()) {
                    return CutsceneInfo.Cutscene.IsPlaying;
                }

                return false;
            }

            /// <summary>
            /// TimeScaleの変更監視
            /// </summary>
            private void OnChangedTimeScale(float timeScale) {
                if (CutsceneInfo?.Cutscene != null) {
                    CutsceneInfo.Cutscene.SetSpeed(timeScale);
                }
            }

            /// <summary>
            /// 制御対象が有効か
            /// </summary>
            private bool IsCutsceneAlive() {
                if (CutsceneInfo == null || CutsceneInfo.Cutscene == null || CutsceneInfo.Root == null) {
                    return false;
                }

                if (CutsceneInfo.Cutscene is Object unityObject && unityObject == null) {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// プール用のカットシーン情報
        /// </summary>
        internal class CutsceneInfo {
            public GameObject Root;
            public GameObject Prefab;
            public Scene Scene;
            public ICutscene Cutscene;
        }

        private readonly int _poolDefaultCapacity;
        private readonly int _poolMaxCapacity;
        private readonly bool _updateGameTime;
        private readonly KeyedObjectPool<GameObject, CutsceneInfo> _prefabBaseCutscenePools;
        private readonly Dictionary<Scene, CutsceneInfo> _sceneBaseCutsceneInfos = new();
        private readonly List<PlayingInfo> _playingInfos = new();
        private readonly Dictionary<int, PlayingInfo> _playingInfoMap = new();
        private readonly ObjectPool<PlayingInfo> _playingInfoPool;

        private bool _isLateUpdating;
        private int _nextHandleId = 1;
        private Transform _rootTransform;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="updateGameTime">GameTimeを使って更新するか</param>
        /// <param name="poolDefaultCapacity">Poolのデフォルトキャパシティ</param>
        /// <param name="poolMaxCapacity">Poolの最大キャパシティ</param>
        public CutsceneManager(bool updateGameTime = true, int poolDefaultCapacity = 1, int poolMaxCapacity = 10000) {
            _updateGameTime = updateGameTime;
            _poolDefaultCapacity = poolDefaultCapacity;
            _poolMaxCapacity = poolMaxCapacity;

            var root = new GameObject(nameof(CutsceneManager), typeof(CutsceneManagerDispatcher));
            var dispatcher = root.GetComponent<CutsceneManagerDispatcher>();
            dispatcher.Setup(this);
            Object.DontDestroyOnLoad(root);
            _rootTransform = root.transform;
            _playingInfoPool = new(
                () => new PlayingInfo(),
                null, null,
                info => { info.Cleanup(); });

            _prefabBaseCutscenePools = new KeyedObjectPool<GameObject, CutsceneInfo>(prefab => {
                var pool = new ObjectPool<CutsceneInfo, GameObject>(prefab,
                    createFunc: pfb => {
                        var instance = Object.Instantiate(pfb, _rootTransform);
                        var cutscene = instance.GetComponent<ICutscene>();
                        if (cutscene == null) {
                            var playableDirector = instance.GetComponent<PlayableDirector>();
                            cutscene = new RuntimeCutscene(playableDirector);
                        }

                        instance.SetActive(false);

                        // Cutscene初期化
                        cutscene.Initialize(_updateGameTime);

                        return new CutsceneInfo { Root = instance, Prefab = prefab, Cutscene = cutscene };
                    },
                    actionOnGet: null,
                    actionOnRelease: (_, info) => {
                        info.Cutscene.OnReturn();
                        info.Root.SetActive(false);
                    },
                    actionOnDestroy: (_, info) => {
                        info.Cutscene.Dispose();
                        Object.Destroy(info.Root);
                    }, true, _poolDefaultCapacity, _poolMaxCapacity);
                return pool;
            });
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            Clear();

            _playingInfoPool.Clear();
            if (_rootTransform != null) {
                Object.Destroy(_rootTransform.gameObject);
                _rootTransform = null;
            }
        }

        /// <inheritdoc/>
        protected override void LateUpdateInternal() {
            _isLateUpdating = true;
            try {
                // 再生中情報の更新
                for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                    var info = _playingInfos[i];

                    // 更新処理
                    if (!info.Update() || info.PendingRelease) {
                        // 廃棄対象ならPoolに戻す
                        _playingInfos.RemoveAt(i);
                        ReleasePlayingInfo(info);
                    }
                }
            }
            finally {
                _isLateUpdating = false;
            }
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="position">初期座標</param>
        /// <param name="rotation">初期向き</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle GetHandle(GameObject prefab, Vector3 position, Quaternion rotation, LayeredTime layeredTime = null) {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(prefab, layeredTime, false);
            if (playingInfo == null) {
                return new Handle();
            }

            // 初期化
            var trans = playingInfo.CutsceneInfo.Root.transform;
            trans.position = position;
            trans.rotation = rotation;
            // Handle化して返却
            return new Handle(this, playingInfo.HandleId);
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle GetHandle(GameObject prefab, LayeredTime layeredTime = null) {
            return GetHandle(prefab, Vector3.zero, Quaternion.identity, layeredTime);
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="position">初期座標</param>
        /// <param name="rotation">初期向き</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle GetHandle<T>(Scene scene, Vector3 position, Quaternion rotation, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(scene, layeredTime, false);
            if (playingInfo == null) {
                return new Handle();
            }

            // 初期化
            var trans = playingInfo.CutsceneInfo.Root.transform;
            trans.position = position;
            trans.rotation = rotation;
            onSetup?.Invoke(playingInfo.CutsceneInfo.Cutscene as T);
            // Handle化して返却
            return new Handle(this, playingInfo.HandleId);
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle GetHandle<T>(Scene scene, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            return GetHandle(scene, Vector3.zero, Quaternion.identity, onSetup, layeredTime);
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle GetHandle(Scene scene, LayeredTime layeredTime = null) {
            return GetHandle<ICutscene>(scene, Vector3.zero, Quaternion.identity, null, layeredTime);
        }

        /// <summary>
        /// Prefabを元にカットシーンを再生
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="position">初期座標</param>
        /// <param name="rotation">初期向き</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Play<T>(GameObject prefab, Vector3 position, Quaternion rotation, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(prefab, layeredTime, true);
            if (playingInfo == null) {
                return new Handle();
            }

            // 初期化
            var trans = playingInfo.CutsceneInfo.Root.transform;
            trans.position = position;
            trans.rotation = rotation;
            // 再生
            playingInfo.Play(onSetup);
            // Handle化して返却
            return new Handle(this, playingInfo.HandleId);
        }

        /// <summary>
        /// Prefabを元にカットシーンを再生
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Play<T>(GameObject prefab, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            return Play(prefab, Vector3.zero, Quaternion.identity, onSetup, layeredTime);
        }

        /// <summary>
        /// Prefabを元にカットシーンを再生
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Play(GameObject prefab, LayeredTime layeredTime = null) {
            return Play<ICutscene>(prefab, Vector3.zero, Quaternion.identity, null, layeredTime);
        }

        /// <summary>
        /// Sceneを元にカットシーンを再生
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="position">初期座標</param>
        /// <param name="rotation">初期向き</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Play<T>(Scene scene, Vector3 position, Quaternion rotation, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(scene, layeredTime, true);
            if (playingInfo == null) {
                return new Handle();
            }

            // 初期化
            var trans = playingInfo.CutsceneInfo.Root.transform;
            trans.position = position;
            trans.rotation = rotation;
            // 再生
            playingInfo.Play(onSetup);
            // Handle化して返却
            return new Handle(this, playingInfo.HandleId);
        }

        /// <summary>
        /// Sceneを元にカットシーンを再生
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Play<T>(Scene scene, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            return Play(scene, Vector3.zero, Quaternion.identity, onSetup, layeredTime);
        }

        /// <summary>
        /// Sceneを元にカットシーンを再生
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Play(Scene scene, LayeredTime layeredTime = null) {
            return Play<ICutscene>(scene, Vector3.zero, Quaternion.identity, null, layeredTime);
        }

        /// <summary>
        /// 再生しているカットシーンとPoolの状態をクリア
        /// </summary>
        public void Clear() {
            // Poolに全部戻して削除
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];
                info.Cleanup();

                // 未使用リストに戻す
                _playingInfos.RemoveAt(i);
                ReleasePlayingInfo(info);
            }

            // Poolを全部削除
            _prefabBaseCutscenePools.ClearAll();

            // Cutsceneを全部削除
            foreach (var info in _sceneBaseCutsceneInfos.Values) {
                if (info?.Cutscene == null || info.Root == null) {
                    continue;
                }

                if (info.Cutscene is Object unityObject && unityObject == null) {
                    continue;
                }

                info.Cutscene.Dispose();
            }
            
            _sceneBaseCutsceneInfos.Clear();
        }

        /// <summary>
        /// 再生情報の生成
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="layeredTime">再生速度をコントロールするためのLayeredTime</param>
        /// <param name="autoDispose">再生完了時に自動で廃棄するか</param>
        private PlayingInfo CreatePlayingInfo(GameObject prefab, LayeredTime layeredTime, bool autoDispose) {
            // Instance生成
            var cutsceneInfo = GetCutsceneInfo(prefab);
            if (cutsceneInfo == null) {
                return null;
            }

            // 再生情報の構築
            var handleId = _nextHandleId++;
            var playingInfo = _playingInfoPool.Get();
            playingInfo.Setup(cutsceneInfo, layeredTime, handleId, autoDispose);
            _playingInfos.Add(playingInfo);
            _playingInfoMap.Add(handleId, playingInfo);

            return playingInfo;
        }

        /// <summary>
        /// 再生情報の生成
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="layeredTime">再生速度をコントロールするためのLayeredTime</param>
        /// <param name="autoDispose">再生完了時に自動で廃棄するか</param>
        private PlayingInfo CreatePlayingInfo(Scene scene, LayeredTime layeredTime, bool autoDispose) {
            // Instance生成
            var cutsceneInfo = GetCutsceneInfo(scene);
            if (cutsceneInfo == null) {
                return null;
            }

            // Scene ベースは常に単一インスタンスなので、既存の制御情報があれば先に解放する
            var oldPlayingInfo = _playingInfos.FirstOrDefault(x => x.CutsceneInfo == cutsceneInfo);
            if (oldPlayingInfo != null) {
                oldPlayingInfo.Cleanup();
                _playingInfos.Remove(oldPlayingInfo);
                ReleasePlayingInfo(oldPlayingInfo);
            }

            // 再生情報の構築
            var handleId = _nextHandleId++;
            var playingInfo = _playingInfoPool.Get();
            playingInfo.Setup(cutsceneInfo, layeredTime, handleId, autoDispose);
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
            if (!TryGetPlayingInfo(handleId, out var playingInfo) || !playingInfo.Initialized) {
                return;
            }

            playingInfo.Cleanup();
            if (_isLateUpdating) {
                playingInfo.MarkPendingRelease();
                return;
            }

            if (_playingInfos.Remove(playingInfo)) {
                ReleasePlayingInfo(playingInfo);
            }
        }

        /// <summary>
        /// PlayingInfoの返却
        /// </summary>
        private void ReleasePlayingInfo(PlayingInfo playingInfo) {
            if (playingInfo == null) {
                return;
            }

            _playingInfoMap.Remove(playingInfo.HandleId);
            var cutsceneInfo = playingInfo.ReleaseCutsceneInfo();
            _playingInfoPool.Release(playingInfo);
            ReturnCutsceneInfo(cutsceneInfo);
        }

        /// <summary>
        /// CutsceneInfoの取得
        /// </summary>
        private CutsceneInfo GetCutsceneInfo(GameObject prefab) {
            if (prefab == null) {
                throw new ArgumentNullException(nameof(prefab));
            }

            return _prefabBaseCutscenePools.Get(prefab);
        }

        /// <summary>
        /// CutsceneInfoの取得
        /// </summary>
        private CutsceneInfo GetCutsceneInfo(Scene scene) {
            if (!scene.IsValid()) {
                throw new ArgumentException("Scene is invalid.", nameof(scene));
            }

            // CutsceneInfoが作られていなければ、ここで生成
            if (!_sceneBaseCutsceneInfos.TryGetValue(scene, out var info)) {
                info = CreateCutsceneInfo(scene);
                _sceneBaseCutsceneInfos[scene] = info;
            }

            return info;
        }

        /// <summary>
        /// CutsceneInfoの返却
        /// </summary>
        private void ReturnCutsceneInfo(CutsceneInfo cutsceneInfo) {
            if (cutsceneInfo == null) {
                return;
            }

            if (cutsceneInfo.Prefab != null) {
                _prefabBaseCutscenePools.Release(cutsceneInfo.Prefab, cutsceneInfo);
            }

            if (cutsceneInfo.Scene.IsValid()) {
                if (!_sceneBaseCutsceneInfos.TryGetValue(cutsceneInfo.Scene, out var info)) {
                    return;
                }

                if (info.Cutscene == null || info.Root == null) {
                    return;
                }

                if (info.Cutscene is Object unityObject && unityObject == null) {
                    return;
                }

                info.Cutscene.OnReturn();
                info.Root.SetActive(false);
            }
        }

        /// <summary>
        /// CutsceneInfoの生成
        /// </summary>
        private CutsceneInfo CreateCutsceneInfo(Scene scene) {
            var rootObjects = scene.GetRootGameObjects();
            var instance = default(GameObject);
            var cutscene = rootObjects
                .Select(x => x.GetComponent<ICutscene>())
                .FirstOrDefault(x => x != null);
            if (cutscene == null) {
                var playableDirector = rootObjects
                    .Select(x => x.GetComponent<PlayableDirector>())
                    .FirstOrDefault(x => x != null);
                if (playableDirector == null) {
                    throw new Exception("Playable director not found.");
                }

                cutscene = new RuntimeCutscene(playableDirector);
                instance = playableDirector.gameObject;
            }
            else {
                if (cutscene is not MonoBehaviour cutsceneBehaviour) {
                    throw new InvalidOperationException("Scene cutscene must be implemented by a MonoBehaviour.");
                }

                instance = cutsceneBehaviour.gameObject;
            }

            instance.SetActive(false);

            // Cutscene初期化
            cutscene.Initialize(_updateGameTime);

            return new CutsceneInfo { Root = instance, Scene = scene, Cutscene = cutscene };
        }
    }
}
