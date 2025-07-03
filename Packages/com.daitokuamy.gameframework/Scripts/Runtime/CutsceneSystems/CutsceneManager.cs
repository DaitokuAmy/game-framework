using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.CutsceneSystems {
    /// <summary>
    /// カットシーン管理クラス
    /// </summary>
    public class CutsceneManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 再生管理用ハンドル
        /// </summary>
        public struct Handle : IDisposable, IEventProcess {
            private PlayingInfo _playingInfo;

            /// <summary>再生中か</summary>
            public bool IsPlaying => _playingInfo != null && _playingInfo.IsPlaying();

            /// <summary>完了しているか</summary>
            public bool IsDone => !IsPlaying;

            /// <summary>未使用</summary>
            object IEnumerator.Current => null;

            /// <summary>エラー</summary>
            Exception IProcess.Exception => null;

            /// <summary>終了通知</summary>
            event Action IEventProcess.ExitEvent {
                add {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.StopEvent += value;
                }
                remove {
                    if (_playingInfo == null) {
                        return;
                    }

                    _playingInfo.StopEvent -= value;
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo playingInfo) {
                _playingInfo = playingInfo;
            }

            /// <summary>
            /// 再生
            /// </summary>
            public void Play<T>(Action<T> onSetup)
                where T : ICutscene {
                if (_playingInfo == null) {
                    return;
                }

                _playingInfo.Play(onSetup);
            }

            /// <summary>
            /// 再生
            /// </summary>
            public void Play() {
                if (_playingInfo == null) {
                    return;
                }

                _playingInfo.Play<ICutscene>(null);
            }

            /// <summary>
            /// 停止
            /// </summary>
            public void Stop(bool autoDispose = false) {
                if (_playingInfo == null) {
                    return;
                }

                _playingInfo.Stop(autoDispose);
            }

            /// <summary>
            /// 時間の設定
            /// </summary>
            public void SetTime(float time) {
                if (_playingInfo == null) {
                    return;
                }

                _playingInfo.SetTime(time);
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (_playingInfo == null) {
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
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        internal class PlayingInfo {
            private LayeredTime _layeredTime;
            private bool _autoDispose;
            private bool _initialized;

            /// <summary>再生通知</summary>
            public event Action PlayEvent;

            /// <summary>停止通知</summary>
            public event Action StopEvent;

            /// <summary>制御対象</summary>
            public CutsceneInfo CutsceneInfo { get; private set; }

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Setup(CutsceneInfo cutsceneInfo, LayeredTime layeredTime, bool autoDispose) {
                if (_initialized) {
                    return;
                }

                _initialized = true;
                CutsceneInfo = cutsceneInfo;
                _layeredTime = layeredTime;
                _autoDispose = autoDispose;

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
                if (!_initialized) {
                    return;
                }

                Stop(true);
                if (_layeredTime != null) {
                    _layeredTime.ChangedTimeScaleEvent -= OnChangedTimeScale;
                }

                _initialized = false;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public bool Update() {
                if (!_initialized) {
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
                    }
                }

                // 自動廃棄処理
                if (_autoDispose && !playing) {
                    Cleanup();
                }

                // Cleanupされていなければ true
                return _initialized;
            }

            /// <summary>
            /// 再生処理
            /// </summary>
            public void Play<T>(Action<T> onSetup)
                where T : ICutscene {
                if (!_initialized) {
                    return;
                }

                if (CutsceneInfo.Cutscene == null) {
                    return;
                }

                if (CutsceneInfo.Cutscene.IsPlaying) {
                    return;
                }

                CutsceneInfo.Root.SetActive(true);

                if (onSetup != null && CutsceneInfo.Cutscene is T cutscene) {
                    onSetup.Invoke(cutscene);
                }

                CutsceneInfo.Cutscene.Play();

                PlayEvent?.Invoke();
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop(bool autoDispose) {
                if (!_initialized) {
                    return;
                }

                // 停止時にAutoDisposeが指定されたら上書きする
                _autoDispose |= autoDispose;

                if (CutsceneInfo.Cutscene == null) {
                    return;
                }

                if (!CutsceneInfo.Cutscene.IsPlaying) {
                    return;
                }

                CutsceneInfo.Cutscene.Stop();
                StopEvent?.Invoke();
            }

            /// <summary>
            /// 時間の設定
            /// </summary>
            public void SetTime(float time) {
                if (!_initialized) {
                    return;
                }

                if (CutsceneInfo.Cutscene == null) {
                    return;
                }

                CutsceneInfo.Cutscene.Seek(time);
            }

            /// <summary>
            /// 再生中か
            /// </summary>
            public bool IsPlaying() {
                if (!_initialized) {
                    return false;
                }

                if (CutsceneInfo.Cutscene != null) {
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
        private readonly Dictionary<GameObject, ObjectPool<CutsceneInfo>> _prefabBaseCutscenePools = new();
        private readonly Dictionary<Scene, CutsceneInfo> _sceneBaseCutsceneInfos = new();
        private readonly List<PlayingInfo> _playingInfos = new();
        private readonly ObjectPool<PlayingInfo> _playingInfoPool;

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
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            Clear();

            _playingInfoPool.Dispose();
            if (_rootTransform != null) {
                Object.Destroy(_rootTransform.gameObject);
                _rootTransform = null;
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
                if (!info.Update()) {
                    // 廃棄対象ならPoolに戻す
                    ReleasePlayingInfo(info);
                    ReturnCutsceneInfo(info.CutsceneInfo);
                }
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
            return new Handle(playingInfo);
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
            return new Handle(playingInfo);
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
            return new Handle(playingInfo);
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
            return new Handle(playingInfo);
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
        /// 再生しているエフェクトとPoolの状態をクリア
        /// </summary>
        public void Clear() {
            // Poolに全部戻して削除
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];
                info.Cleanup();

                // 未使用リストに戻す
                _playingInfoPool.Release(info);
                ReturnCutsceneInfo(info.CutsceneInfo);
            }

            // Poolを全部削除
            foreach (var pool in _prefabBaseCutscenePools.Values) {
                pool.Dispose();
            }

            // Cutsceneを全部削除
            foreach (var info in _sceneBaseCutsceneInfos.Values) {
                info.Cutscene.Dispose();
            }

            _prefabBaseCutscenePools.Clear();
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
            var playingInfo = _playingInfoPool.Get();
            playingInfo.Setup(cutsceneInfo, layeredTime, autoDispose);
            _playingInfos.Add(playingInfo);

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

            // 再生中なら停止して再実行
            if (cutsceneInfo.Cutscene.IsPlaying) {
                var oldPlayingInfo = _playingInfos.FirstOrDefault(x => x.CutsceneInfo == cutsceneInfo);
                if (oldPlayingInfo != null) {
                    ReleasePlayingInfo(oldPlayingInfo);
                    ReturnCutsceneInfo(oldPlayingInfo.CutsceneInfo);
                }
            }

            // 再生情報の構築
            var playingInfo = _playingInfoPool.Get();
            playingInfo.Setup(cutsceneInfo, layeredTime, autoDispose);
            _playingInfos.Add(playingInfo);

            return playingInfo;
        }

        /// <summary>
        /// PlayingInfoの返却
        /// </summary>
        private void ReleasePlayingInfo(PlayingInfo playingInfo) {
            playingInfo.Cleanup();
            _playingInfos.Remove(playingInfo);
            _playingInfoPool.Release(playingInfo);
        }

        /// <summary>
        /// CutsceneInfoの取得
        /// </summary>
        private CutsceneInfo GetCutsceneInfo(GameObject prefab) {
            if (prefab == null) {
                throw new ArgumentNullException($"Prefab is null");
            }

            // Poolが作られていなければ、ここで生成
            if (!_prefabBaseCutscenePools.TryGetValue(prefab, out var pool)) {
                pool = CreatePool(prefab);
                _prefabBaseCutscenePools[prefab] = pool;
            }

            return pool.Get();
        }

        /// <summary>
        /// CutsceneInfoの取得
        /// </summary>
        private CutsceneInfo GetCutsceneInfo(Scene scene) {
            if (!scene.IsValid()) {
                throw new ArgumentNullException($"Scene is invalid");
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
            if (cutsceneInfo.Prefab != null) {
                if (!_prefabBaseCutscenePools.TryGetValue(cutsceneInfo.Prefab, out var pool)) {
                    return;
                }

                pool.Release(cutsceneInfo);
            }

            if (cutsceneInfo.Scene.IsValid()) {
                if (!_sceneBaseCutsceneInfos.TryGetValue(cutsceneInfo.Scene, out var info)) {
                    return;
                }

                info.Cutscene.OnReturn();
                info.Root.SetActive(false);
            }
        }

        /// <summary>
        /// Poolの生成
        /// </summary>
        private ObjectPool<CutsceneInfo> CreatePool(GameObject prefab) {
            var pool = new ObjectPool<CutsceneInfo>(() => {
                    var instance = Object.Instantiate(prefab, _rootTransform);
                    var cutscene = instance.GetComponent<ICutscene>();
                    if (cutscene == null) {
                        var playableDirector = instance.GetComponent<PlayableDirector>();
                        cutscene = new RuntimeCutscene(playableDirector);
                    }

                    instance.SetActive(false);

                    // Cutscene初期化
                    cutscene.Initialize(_updateGameTime);

                    return new CutsceneInfo {
                        Root = instance,
                        Prefab = prefab,
                        Cutscene = cutscene
                    };
                }, _ => { },
                info => {
                    info.Cutscene.OnReturn();
                    info.Root.SetActive(false);
                },
                info => {
                    info.Cutscene.Dispose();
                    Object.Destroy(info.Root);
                }, true, _poolDefaultCapacity, _poolMaxCapacity);

            return pool;
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
                instance = ((Cutscene)cutscene).gameObject;
            }

            instance.SetActive(false);

            // Cutscene初期化
            cutscene.Initialize(_updateGameTime);

            return new CutsceneInfo {
                Root = instance,
                Scene = scene,
                Cutscene = cutscene
            };
        }
    }
}