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
        public struct Handle : IDisposable, IProcess {
            // 再生中の情報
            private PlayingInfo _playingInfo;

            // 再生中か
            public bool IsPlaying => _playingInfo != null && _playingInfo.IsPlaying();
            // 未使用
            object IEnumerator.Current => null;
            // 完了しているか
            bool IProcess.IsDone => !IsPlaying;
            // エラー
            Exception IProcess.Exception => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(PlayingInfo playingInfo) {
                _playingInfo = playingInfo;
            }

            /// <summary>
            /// イベント通知の監視
            /// </summary>
            /// <param name="onPlay">再生開始通知</param>
            /// <param name="onStop">再生停止通知</param>
            public void BindEvents(Action onPlay, Action onStop) {
                if (_playingInfo == null) {
                    return;
                }

                if (onPlay != null) {
                    _playingInfo.OnPlayEvent += onPlay;
                }

                if (onStop != null) {
                    _playingInfo.OnStopEvent += onStop;
                }
            }

            /// <summary>
            /// 再生
            /// </summary>
            public void Play() {
                if (_playingInfo == null) {
                    return;
                }

                _playingInfo.Play();
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
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (_playingInfo == null) {
                    return;
                }

                _playingInfo.Dispose();
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
        internal class PlayingInfo : IDisposable {
            // TimeScale変更用LayeredTime
            private readonly LayeredTime _layeredTime;
            // 自動廃棄するか
            private bool _autoDispose;

            // 再生通知
            public event Action OnPlayEvent;
            // 停止通知
            public event Action OnStopEvent;

            // 制御対象
            public CutsceneInfo CutsceneInfo { get; private set; }
            // 廃棄済みか
            public bool Disposed { get; private set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public PlayingInfo(CutsceneInfo cutsceneInfo, LayeredTime layeredTime, bool autoDispose) {
                CutsceneInfo = cutsceneInfo;
                _layeredTime = layeredTime;
                _autoDispose = autoDispose;

                if (layeredTime != null) {
                    layeredTime.OnChangedTimeScale += OnChangedTimeScale;
                    OnChangedTimeScale(layeredTime.TimeScale);
                }
                else {
                    OnChangedTimeScale(1.0f);
                }
            }

            /// <summary>
            /// 廃棄処理
            /// </summary>
            public void Dispose() {
                if (Disposed) {
                    return;
                }

                Stop(true);
                if (_layeredTime != null) {
                    _layeredTime.OnChangedTimeScale += OnChangedTimeScale;
                }

                Disposed = true;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public void Update() {
                if (Disposed) {
                    return;
                }

                var deltaTime = _layeredTime?.DeltaTime ?? Time.deltaTime;
                var playing = CutsceneInfo.cutscene?.IsPlaying ?? false;
                if (playing) {
                    // Cutsceneの更新
                    CutsceneInfo.cutscene.Update(deltaTime);

                    // 再生停止
                    playing = CutsceneInfo.cutscene.IsPlaying;
                    if (!playing) {
                        CutsceneInfo.cutscene.Stop();
                        OnStopEvent?.Invoke();
                    }
                }

                // 自動廃棄処理
                if (_autoDispose && !playing) {
                    Dispose();
                }
            }

            /// <summary>
            /// 再生処理
            /// </summary>
            public void Play() {
                if (Disposed) {
                    return;
                }

                if (CutsceneInfo.cutscene == null) {
                    return;
                }

                if (CutsceneInfo.cutscene.IsPlaying) {
                    return;
                }

                CutsceneInfo.root.SetActive(true);
                CutsceneInfo.cutscene.Play();

                OnPlayEvent?.Invoke();
            }

            /// <summary>
            /// 停止処理
            /// </summary>
            public void Stop(bool autoDispose) {
                if (Disposed) {
                    return;
                }

                // 停止時にAutoDisposeが指定されたら上書きする
                _autoDispose |= autoDispose;

                if (CutsceneInfo.cutscene == null) {
                    return;
                }

                if (!CutsceneInfo.cutscene.IsPlaying) {
                    return;
                }

                CutsceneInfo.cutscene.Stop();
                OnStopEvent?.Invoke();
            }

            /// <summary>
            /// 再生中か
            /// </summary>
            public bool IsPlaying() {
                if (Disposed) {
                    return false;
                }

                if (CutsceneInfo.cutscene != null) {
                    return CutsceneInfo.cutscene.IsPlaying;
                }

                return false;
            }

            /// <summary>
            /// TimeScaleの変更監視
            /// </summary>
            private void OnChangedTimeScale(float timeScale) {
                if (CutsceneInfo?.cutscene != null) {
                    CutsceneInfo.cutscene.SetSpeed(timeScale);
                }
            }
        }

        /// <summary>
        /// プール用のカットシーン情報
        /// </summary>
        internal class CutsceneInfo {
            public GameObject root;
            public GameObject prefab;
            public Scene scene;
            public ICutscene cutscene;
        }

        // Poolキャパシティ
        private readonly int _poolDefaultCapacity;
        private readonly int _poolMaxCapacity;
        // GameTimeによる更新モード
        private readonly bool _updateGameTime;

        // インスタンス格納用のTransform
        private Transform _rootTransform;
        // インスタンスキャッシュ用のPool
        private readonly Dictionary<GameObject, ObjectPool<CutsceneInfo>> _prefabBaseCutscenePools = new();
        private readonly Dictionary<Scene, CutsceneInfo> _sceneBaseCutsceneInfos = new();
        // 管理用再生中情報
        private List<PlayingInfo> _playingInfos = new();

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
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="position">初期座標</param>
        /// <param name="rotation">初期向き</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(prefab, layeredTime, false);
            if (playingInfo == null) {
                return new Handle();
            }

            // 初期化
            var trans = playingInfo.CutsceneInfo.root.transform;
            trans.position = position;
            trans.rotation = rotation;
            onSetup?.Invoke(playingInfo.CutsceneInfo.cutscene as T);
            // Handle化して返却
            return new Handle(playingInfo);
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Get<T>(GameObject prefab, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            return Get(prefab, Vector3.zero, Quaternion.identity, onSetup, layeredTime);
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="prefab">再生対象のPrefab</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Get(GameObject prefab, LayeredTime layeredTime = null) {
            return Get<ICutscene>(prefab, Vector3.zero, Quaternion.identity, null, layeredTime);
        }

        /// <summary>
        /// シーンタイプのカットシーンを初期化(非アクティブ化)
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        public void Setup(Scene scene) {
            GetCutsceneInfo(scene);
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
        public Handle Get<T>(Scene scene, Vector3 position, Quaternion rotation, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            // 再生情報の生成
            var playingInfo = CreatePlayingInfo(scene, layeredTime, false);
            if (playingInfo == null) {
                return new Handle();
            }

            // 初期化
            var trans = playingInfo.CutsceneInfo.root.transform;
            trans.position = position;
            trans.rotation = rotation;
            onSetup?.Invoke(playingInfo.CutsceneInfo.cutscene as T);
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
        public Handle Get<T>(Scene scene, Action<T> onSetup, LayeredTime layeredTime = null)
            where T : class, ICutscene {
            return Get(scene, Vector3.zero, Quaternion.identity, onSetup, layeredTime);
        }

        /// <summary>
        /// インスタンスの取得(再生はコールせずに自分でハンドリングする)
        /// ※使用が終わった場合、HandleをDisposeしてください
        /// </summary>
        /// <param name="scene">再生対象のScene</param>
        /// <param name="layeredTime">再生速度コントロール用LayeredTime</param>
        public Handle Get(Scene scene, LayeredTime layeredTime = null) {
            return Get<ICutscene>(scene, Vector3.zero, Quaternion.identity, null, layeredTime);
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
            var trans = playingInfo.CutsceneInfo.root.transform;
            trans.position = position;
            trans.rotation = rotation;
            onSetup?.Invoke(playingInfo.CutsceneInfo.cutscene as T);
            // 再生
            playingInfo.Play();
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
            var trans = playingInfo.CutsceneInfo.root.transform;
            trans.position = position;
            trans.rotation = rotation;
            onSetup?.Invoke(playingInfo.CutsceneInfo.cutscene as T);
            // 再生
            playingInfo.Play();
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

                // 廃棄
                info.Dispose();

                // 未使用リストに戻す
                _playingInfos.RemoveAt(i);
                ReturnCutsceneInfo(info.CutsceneInfo);
            }

            // Poolを全部削除
            foreach (var pool in _prefabBaseCutscenePools.Values) {
                pool.Dispose();
            }

            // Cutsceneを全部削除
            foreach (var info in _sceneBaseCutsceneInfos.Values) {
                info.cutscene.Dispose();
            }

            _prefabBaseCutscenePools.Clear();
            _sceneBaseCutsceneInfos.Clear();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            Clear();

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
                if (info.Disposed) {
                    _playingInfos.RemoveAt(i);
                    ReturnCutsceneInfo(info.CutsceneInfo);
                }
            }
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
            var playingInfo = new PlayingInfo(cutsceneInfo, layeredTime, autoDispose);
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
            if (cutsceneInfo.cutscene.IsPlaying) {
                var oldPlayingInfo = _playingInfos.FirstOrDefault(x => x.CutsceneInfo == cutsceneInfo);
                if (oldPlayingInfo != null) {
                    oldPlayingInfo.Dispose();
                    _playingInfos.Remove(oldPlayingInfo);
                    ReturnCutsceneInfo(oldPlayingInfo.CutsceneInfo);
                }
            }

            // 再生情報の構築
            var playingInfo = new PlayingInfo(cutsceneInfo, layeredTime, autoDispose);
            _playingInfos.Add(playingInfo);

            return playingInfo;
        }

        /// <summary>
        /// CutsceneInfoの取得
        /// </summary>
        private CutsceneInfo GetCutsceneInfo(GameObject prefab) {
            if (prefab == null) {
                Debug.unityLogger.LogError(nameof(CutsceneManager), "prefab is null.");
                return null;
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
                Debug.unityLogger.LogError(nameof(CutsceneManager), "scene is invalid.");
                return null;
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
            if (cutsceneInfo.prefab != null) {
                if (!_prefabBaseCutscenePools.TryGetValue(cutsceneInfo.prefab, out var pool)) {
                    Debug.unityLogger.LogWarning(nameof(CutsceneManager), $"Not found cutscene pool. {cutsceneInfo.prefab.name}");
                    return;
                }

                pool.Release(cutsceneInfo);
            }

            if (cutsceneInfo.scene.IsValid()) {
                if (!_sceneBaseCutsceneInfos.TryGetValue(cutsceneInfo.scene, out var info)) {
                    Debug.unityLogger.LogWarning(nameof(CutsceneManager), $"Not found cutscene info. {cutsceneInfo.scene.name}");
                    return;
                }

                info.cutscene.OnReturn();
                info.root.SetActive(false);
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
                        root = instance,
                        prefab = prefab,
                        cutscene = cutscene
                    };
                }, _ => { },
                info => {
                    info.cutscene.OnReturn();
                    info.root.SetActive(false);
                },
                info => {
                    info.cutscene.Dispose();
                    Object.Destroy(info.root);
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
                root = instance,
                scene = scene,
                cutscene = cutscene
            };
        }
    }
}