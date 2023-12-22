using System;
using System.Collections.Generic;
using GameFramework.CollisionSystems;
using GameFramework.Core;
using GameFramework.TaskSystems;
using UnityEngine;
using UnityEngine.Pool;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体オブジェクト管理クラス
    /// </summary>
    public class ProjectileObjectManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 更新モード
        /// </summary>
        public enum UpdateMode {
            Update,
            LateUpdate,
        }

        /// <summary>
        /// 飛翔オブジェクト用ハンドル
        /// </summary>
        public struct Handle : IDisposable {
            private ProjectilePlayer.Handle _projectileHandle;

            // 有効なハンドルか
            public bool IsValid => _projectileHandle.IsValid;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            internal Handle(ProjectilePlayer.Handle projectileHandle) {
                _projectileHandle = projectileHandle;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                if (!IsValid) {
                    return;
                }

                _projectileHandle.Stop();
            }
        }

        /// <summary>
        /// プール用Objectの情報
        /// </summary>
        private class BulletObjectInfo {
            public GameObject prefab;
            public IBulletProjectileObject projectileObject;
        }

        /// <summary>
        /// プール用Objectの情報
        /// </summary>
        private class BeamObjectInfo {
            public GameObject prefab;
            public IBeamProjectileObject projectileObject;
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        private abstract class PlayingInfo {
            // TimeScale変更用LayeredTime
            private LayeredTime _layeredTime;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public PlayingInfo(LayeredTime layeredTime) {
                _layeredTime = layeredTime;
                if (_layeredTime != null) {
                    _layeredTime.OnChangedTimeScale += OnChangedTimeScale;
                }
            }

            /// <summary>
            /// Poolへの返却
            /// </summary>
            public void Release() {
                if (_layeredTime != null) {
                    _layeredTime.OnChangedTimeScale -= OnChangedTimeScale;
                    _layeredTime = null;
                }

                ReleaseInternal();
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            /// <returns>再生終了したらfalse</returns>
            public bool Update() {
                var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;
                return UpdateInternal(deltaTime);
            }

            protected abstract void OnChangedTimeScale(float timeScale);
            protected abstract void ReleaseInternal();
            protected abstract bool UpdateInternal(float deltaTime);

            /// <summary>
            /// Layerの再帰的な設定
            /// </summary>
            protected void SetLayer(Transform trans, int layer) {
                if (trans == null) {
                    return;
                }

                trans.gameObject.layer = layer;
                foreach (Transform child in trans) {
                    SetLayer(child, layer);
                }
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        private class BulletPlayingInfo : PlayingInfo {
            private readonly ObjectPool<BulletObjectInfo> _pool;
            private readonly BulletObjectInfo _objectInfo;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public BulletPlayingInfo(ObjectPool<BulletObjectInfo> pool, BulletObjectInfo objectInfo, LayeredTime layeredTime, int layer)
                : base(layeredTime) {
                _pool = pool;
                _objectInfo = objectInfo;
                SetLayer(objectInfo.projectileObject.transform, layer);
            }

            /// <summary>
            /// TimeScaleの変更通知
            /// </summary>
            protected override void OnChangedTimeScale(float timeScale) {
                _objectInfo.projectileObject.SetSpeed(timeScale);
            }

            /// <summary>
            /// Poolへの返却
            /// </summary>
            protected override void ReleaseInternal() {
                _pool.Release(_objectInfo);
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                _objectInfo.projectileObject.Update(deltaTime);
                return _objectInfo.projectileObject.IsPlaying;
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        private class BeamPlayingInfo : PlayingInfo {
            private readonly ObjectPool<BeamObjectInfo> _pool;
            private readonly BeamObjectInfo _objectInfo;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public BeamPlayingInfo(ObjectPool<BeamObjectInfo> pool, BeamObjectInfo objectInfo, LayeredTime layeredTime, int layer)
                : base(layeredTime) {
                _pool = pool;
                _objectInfo = objectInfo;
                SetLayer(objectInfo.projectileObject.transform, layer);
            }

            /// <summary>
            /// TimeScaleの変更通知
            /// </summary>
            protected override void OnChangedTimeScale(float timeScale) {
                _objectInfo.projectileObject.SetSpeed(timeScale);
            }

            /// <summary>
            /// Poolへの返却
            /// </summary>
            protected override void ReleaseInternal() {
                _pool.Release(_objectInfo);
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                _objectInfo.projectileObject.Update(deltaTime);
                return _objectInfo.projectileObject.IsPlaying;
            }
        }

        // Poolキャパシティ
        private readonly int _poolDefaultCapacity;
        private readonly int _poolMaxCapacity;

        // コリジョンマネージャ
        private CollisionManager _collisionManager;
        // 更新モード
        private UpdateMode _updateMode;

        // 管理ルートになるTransform
        private Transform _rootTransform;
        // Projectile再生用Player
        private ProjectilePlayer _projectilePlayer;
        // ProjectileObjectPool管理用
        private Dictionary<GameObject, ObjectPool<BulletObjectInfo>> _bulletPools = new();
        private Dictionary<GameObject, ObjectPool<BeamObjectInfo>> _beamPools = new();
        // 再生中情報
        private List<PlayingInfo> _playingInfos = new();
        // Poolを有効にするフラグ
        private bool _activePool = true;

        /// <summary>デフォルト指定のLayer</summary>
        public int DefaultLayer { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="collisionManager">衝突判定に使うCollisionManager</param>
        /// <param name="updateMode">更新モード</param>
        /// <param name="poolDefaultCapacity">Poolのデフォルトキャパシティ</param>
        /// <param name="poolMaxCapacity">Poolの最大キャパシティ</param>
        public ProjectileObjectManager(CollisionManager collisionManager, UpdateMode updateMode = UpdateMode.Update, int poolDefaultCapacity = 10, int poolMaxCapacity = 10000) {
            _collisionManager = collisionManager;
            _updateMode = updateMode;
            _poolDefaultCapacity = poolDefaultCapacity;
            _poolMaxCapacity = poolMaxCapacity;
            _projectilePlayer = new ProjectilePlayer();

            // Rootの生成
            var root = new GameObject("ProjectileObjectManager", typeof(ProjectileObjectManagerDispatcher));
            var dispatcher = root.GetComponent<ProjectileObjectManagerDispatcher>();
            dispatcher.Setup(this);
            UnityEngine.Object.DontDestroyOnLoad(root);
            _rootTransform = root.transform;
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
        /// 弾オブジェクトの再生
        /// </summary>
        /// <param name="listener">当たり判定通知用リスナー</param>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="scale">拡大率</param>
        /// <param name="hitLayerMask">当たり判定に使うLayerMask</param>
        /// <param name="hitCount">最大衝突回数(-1だと無限)</param>
        /// <param name="customData">当たり判定通知時に使うカスタムデータ</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="layer">指定するレイヤー</param>
        /// <param name="checkHitFunc">当たりとして有効かの判定関数</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onExit">飛翔完了通知</param>
        public Handle Play(IRaycastCollisionListener listener, GameObject prefab, IBulletProjectile projectile, Vector3 scale,
            int hitLayerMask, int hitCount = -1, object customData = null, LayeredTime layeredTime = null, int layer = -1, Func<RaycastHitResult, bool> checkHitFunc = null,
            Action<Vector3, Quaternion> onUpdatedTransform = null,
            Action onExit = null) {
            if (prefab == null) {
                Debug.LogWarning("Projectile object prefab is null.");
                return new Handle();
            }

            // Poolの初期化
            if (!_bulletPools.TryGetValue(prefab, out var pool)) {
                pool = CreateBulletPool(prefab, _activePool);
                _bulletPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var objectInfo = pool.Get();
            var projectileObj = objectInfo.projectileObject;
            projectileObj.SetLocalScale(scale);
            projectileObj.Start(projectile);

            // Raycastの生成
            var raycastCollision = default(IRaycastCollision);
            var collisionHandle = new CollisionHandle();
            var lastHitPoint = default(Vector3?);

            // Projectileを再生
            var projectileHandle = _projectilePlayer.Play(projectile, layeredTime, prj => {
                projectileObj.UpdateProjectile(prj);
                raycastCollision.March(prj.Position);
                onUpdatedTransform?.Invoke(prj.Position, prj.Rotation);
            }, () => {
                collisionHandle.Dispose();
                if (lastHitPoint != null) {
                    projectileObj.SetPosition(lastHitPoint.Value);
                }

                projectileObj.Exit();
                onExit?.Invoke();
            });

            // Raycastの生成
            raycastCollision = projectileObj.RaycastRadius > float.Epsilon
                ? new SphereRaycastCollision(projectile.Position, projectile.Position, projectileObj.RaycastRadius)
                : new LineRaycastCollision(projectile.Position, projectile.Position);
            
            // コリジョン登録
            collisionHandle = _collisionManager.Register(raycastCollision, hitLayerMask, customData, result => {
                if (checkHitFunc != null && !checkHitFunc.Invoke(result)) {
                    return;
                }

                projectileObj.OnHitCollision(result);
                listener.OnHitRaycastCollision(result);
                if (hitCount >= 0 && result.hitCount >= hitCount) {
                    // 着弾位置に移動してから廃棄する
                    if (result.raycastHit.distance > float.Epsilon) {
                        lastHitPoint = result.raycastHit.point;
                        projectileObj.SetPosition(lastHitPoint.Value);
                        projectileHandle.Stop(lastHitPoint);
                    }
                    else {
                        projectileHandle.Stop();
                    }
                }
            });

            if (layer < 0) {
                layer = DefaultLayer;
            }

            // 再生情報として登録
            _playingInfos.Add(new BulletPlayingInfo(pool, objectInfo, layeredTime, layer));

            // ハンドル化して返却
            return new Handle(projectileHandle);
        }

        /// <summary>
        /// 飛翔オブジェクトの再生
        /// </summary>
        /// <param name="listener">当たり判定通知用リスナー</param>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="hitLayerMask">当たり判定に使うLayerMask</param>
        /// <param name="hitCount">最大衝突回数(-1だと無限)</param>
        /// <param name="customData">当たり判定通知時に使うカスタムデータ</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="layer">レイヤー指定</param>
        /// <param name="checkHitFunc">当たりとして有効かの判定関数</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onExit">飛翔完了通知</param>
        public Handle Play(IRaycastCollisionListener listener, GameObject prefab, IBulletProjectile projectile,
            int hitLayerMask, int hitCount = -1, object customData = null, LayeredTime layeredTime = null, int layer = -1, Func<RaycastHitResult, bool> checkHitFunc = null,
            Action<Vector3, Quaternion> onUpdatedTransform = null,
            Action onExit = null) {
            return Play(listener, prefab, projectile, Vector3.one, hitLayerMask, hitCount, customData, layeredTime, layer, checkHitFunc, onUpdatedTransform, onExit);
        }

        /// <summary>
        /// 飛翔オブジェクトの再生
        /// </summary>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="scale">拡大率</param>
        /// <param name="hitLayerMask">当たり判定に使うLayerMask</param>
        /// <param name="hitCount">最大衝突回数(-1だと無限)</param>
        /// <param name="customData">当たり判定通知時に使うカスタムデータ</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="layer">レイヤー指定</param>
        /// <param name="checkHitFunc">当たりとして有効かの判定関数</param>
        /// <param name="onHitRaycastCollision">当たり判定発生時通知</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onExit">飛翔完了通知</param>
        public Handle Play(GameObject prefab, IBulletProjectile projectile, Vector3 scale,
            int hitLayerMask, int hitCount = -1, object customData = null, LayeredTime layeredTime = null, int layer = -1, Func<RaycastHitResult, bool> checkHitFunc = null,
            Action<RaycastHitResult> onHitRaycastCollision = null,
            Action<Vector3, Quaternion> onUpdatedTransform = null,
            Action onExit = null) {
            var listener = new RaycastCollisionListener();
            listener.OnHitRaycastCollisionEvent += onHitRaycastCollision;
            return Play(listener, prefab, projectile, scale, hitLayerMask, hitCount, customData, layeredTime, layer, checkHitFunc,
                onUpdatedTransform, onExit);
        }

        /// <summary>
        /// 飛翔オブジェクトの再生
        /// </summary>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="hitLayerMask">当たり判定に使うLayerMask</param>
        /// <param name="hitCount">最大衝突回数(-1だと無限)</param>
        /// <param name="customData">当たり判定通知時に使うカスタムデータ</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="layer">レイヤー指定</param>
        /// <param name="checkHitFunc">当たりとして有効かの判定関数</param>
        /// <param name="onHitRaycastCollision">当たり判定発生時通知</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onExit">飛翔完了通知</param>
        public Handle Play(GameObject prefab, IBulletProjectile projectile,
            int hitLayerMask, int hitCount = -1, object customData = null, LayeredTime layeredTime = null, int layer = -1, Func<RaycastHitResult, bool> checkHitFunc = null,
            Action<RaycastHitResult> onHitRaycastCollision = null,
            Action<Vector3, Quaternion> onUpdatedTransform = null,
            Action onExit = null) {
            return Play(prefab, projectile, Vector3.one, hitLayerMask, hitCount, customData, layeredTime, layer,
                checkHitFunc,
                onHitRaycastCollision,
                onUpdatedTransform, onExit);
        }

        /// <summary>
        /// 弾オブジェクトの再生
        /// </summary>
        /// <param name="listener">当たり判定通知用リスナー</param>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectile">飛翔アルゴリズム</param>
        /// <param name="scale">拡大率</param>
        /// <param name="hitLayerMask">当たり判定に使うLayerMask</param>
        /// <param name="hitCount">最大衝突回数(-1だと無限)</param>
        /// <param name="customData">当たり判定通知時に使うカスタムデータ</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="layer">レイヤー指定</param>
        /// <param name="checkHitFunc">当たりとして有効かの判定関数</param>
        /// <param name="onUpdatedTransform">座標の更新通知</param>
        /// <param name="onExit">飛翔完了通知</param>
        public Handle Play(IRaycastCollisionListener listener, GameObject prefab, IBeamProjectile projectile, Vector3 scale,
            int hitLayerMask, int hitCount = -1, object customData = null, LayeredTime layeredTime = null, int layer = -1, Func<RaycastHitResult, bool> checkHitFunc = null,
            Action<Vector3, Vector3, Quaternion> onUpdatedTransform = null,
            Action onExit = null) {
            if (prefab == null) {
                Debug.LogWarning("Projectile object prefab is null.");
                return new Handle();
            }

            // Poolの初期化
            if (!_beamPools.TryGetValue(prefab, out var pool)) {
                pool = CreateBeamPool(prefab, _activePool);
                _beamPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var objectInfo = pool.Get();
            var projectileObj = objectInfo.projectileObject;
            projectileObj.SetLocalScale(scale);
            projectileObj.Start(projectile);

            // Raycast用データ
            var raycastCollision = default(IRaycastCollision);
            var collisionHandle = new CollisionHandle();

            // Projectileを再生
            var projectileHandle = _projectilePlayer.Play(projectile, layeredTime, prj => {
                projectileObj.UpdateProjectile(prj);
                if (raycastCollision != null) {
                    raycastCollision.Start = prj.TailPosition;
                    raycastCollision.End = prj.HeadPosition;
                    raycastCollision.IsActive = prj.IsSolid;
                }

                onUpdatedTransform?.Invoke(prj.HeadPosition, prj.TailPosition, prj.Rotation);
            }, () => {
                collisionHandle.Dispose();
                projectileObj.Exit();
                onExit?.Invoke();
            });

            // Raycastの生成
            raycastCollision = projectileObj.RaycastRadius > float.Epsilon
                ? new SphereRaycastCollision(projectile.TailPosition, projectile.HeadPosition, projectileObj.RaycastRadius)
                : new LineRaycastCollision(projectile.TailPosition, projectile.HeadPosition);

            // コリジョン登録
            collisionHandle = _collisionManager.Register(raycastCollision, hitLayerMask, customData, result => {
                if (checkHitFunc != null && !checkHitFunc.Invoke(result)) {
                    return;
                }

                projectileObj.OnHitCollision(result);
                listener.OnHitRaycastCollision(result);
                if (hitCount >= 0 && result.hitCount >= hitCount) {
                    projectileHandle.Stop();
                }
            });

            if (layer < 0) {
                layer = DefaultLayer;
            }

            // 再生情報として登録
            _playingInfos.Add(new BeamPlayingInfo(pool, objectInfo, layeredTime, layer));

            // ハンドル化して返却
            return new Handle(projectileHandle);
        }

        /// <summary>
        /// 全飛翔オブジェクトの停止
        /// </summary>
        /// <param name="clear">即時クリアするか</param>
        public void StopAll(bool clear = false) {
            _projectilePlayer.StopAll(clear);
        }

        /// <summary>
        /// 再生しているエフェクトとPoolの状態をクリア
        /// </summary>
        public void Clear() {
            // 再生状態を停止
            _projectilePlayer.StopAll(true);

            // Poolに返却
            foreach (var info in _playingInfos) {
                info.Release();
            }

            _playingInfos.Clear();

            // Poolを削除
            foreach (var pool in _bulletPools.Values) {
                pool.Dispose();
            }

            foreach (var pool in _beamPools.Values) {
                pool.Dispose();
            }

            _bulletPools.Clear();
            _beamPools.Clear();
        }

        /// <summary>
        /// 更新処理
        /// </summary>tile
        protected override void UpdateInternal() {
            if (_updateMode == UpdateMode.Update) {
                UpdateProjectileObjects();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            if (_updateMode == UpdateMode.LateUpdate) {
                UpdateProjectileObjects();
            }
        }

        /// <summary>
        /// 飛翔体オブジェクトの更新
        /// </summary>
        private void UpdateProjectileObjects() {
            _projectilePlayer.Update();

            // ProjectileObjectの更新
            for (var i = _playingInfos.Count - 1; i >= 0; i--) {
                var info = _playingInfos[i];
                if (!info.Update()) {
                    // 再生完了していたらPoolに返却する
                    info.Release();
                    _playingInfos.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _projectilePlayer.Dispose();

            // Poolに返却
            foreach (var info in _playingInfos) {
                info.Release();
            }

            _playingInfos.Clear();

            // Poolを削除
            foreach (var pool in _bulletPools.Values) {
                pool.Dispose();
            }

            foreach (var pool in _beamPools.Values) {
                pool.Dispose();
            }

            _bulletPools.Clear();
            _beamPools.Clear();

            if (_rootTransform != null) {
                UnityEngine.Object.Destroy(_rootTransform.gameObject);
                _rootTransform = null;
            }
        }

        /// <summary>
        /// BulletPoolの生成
        /// </summary>
        private ObjectPool<BulletObjectInfo> CreateBulletPool(GameObject prefab, bool activePool) {
            // インスタンス生成処理
            void CreateContent(BulletObjectInfo objectInfo) {
                var gameObj = UnityEngine.Object.Instantiate(objectInfo.prefab, _rootTransform);
                var instance = gameObj.GetComponent<IBulletProjectileObject>();
                if (instance == null) {
                    instance = gameObj.AddComponent<BulletProjectileObject>();
                }

                instance.SetActive(false);
                objectInfo.projectileObject = instance;
            }

            // 中身の削除
            void DestroyContent(BulletObjectInfo objectInfo) {
                if (objectInfo == null || objectInfo.projectileObject == null) {
                    return;
                }

                if (objectInfo.projectileObject != null) {
                    objectInfo.projectileObject.Dispose();
                }
                
                objectInfo.projectileObject = null;
            }
            
            var pool = new ObjectPool<BulletObjectInfo>(() => {
                    var objectInfo = new BulletObjectInfo();
                    objectInfo.prefab = prefab;

                    if (activePool) {
                        CreateContent(objectInfo);
                    }

                    return objectInfo;
                }, info => {
                    if (activePool) {
                        info.projectileObject.SetActive(true);
                    }
                    else {
                        CreateContent(info);
                        info.projectileObject.SetActive(true);
                    }
                }, info => {
                    if (activePool) {
                        info.projectileObject.SetActive(false);
                    }
                    else {
                        info.projectileObject.SetActive(false);
                        DestroyContent(info);
                    }
                },
                DestroyContent, true, _poolDefaultCapacity, _poolMaxCapacity);
            return pool;
        }

        /// <summary>
        /// BeamPoolの生成
        /// </summary>
        private ObjectPool<BeamObjectInfo> CreateBeamPool(GameObject prefab, bool activePool) {
            // インスタンス生成処理
            void CreateContent(BeamObjectInfo objectInfo) {
                var gameObj = UnityEngine.Object.Instantiate(objectInfo.prefab, _rootTransform);
                var instance = gameObj.GetComponent<IBeamProjectileObject>();
                if (instance == null) {
                    instance = gameObj.AddComponent<BeamProjectileObject>();
                }

                instance.SetActive(false);
                objectInfo.projectileObject = instance;
            }

            // 中身の削除
            void DestroyContent(BeamObjectInfo objectInfo) {
                if (objectInfo == null || objectInfo.projectileObject == null) {
                    return;
                }

                if (objectInfo.projectileObject != null) {
                    objectInfo.projectileObject.Dispose();
                }
                
                objectInfo.projectileObject = null;
            }
            
            var pool = new ObjectPool<BeamObjectInfo>(() => {
                    var objectInfo = new BeamObjectInfo();
                    objectInfo.prefab = prefab;

                    if (activePool) {
                        CreateContent(objectInfo);
                    }

                    return objectInfo;
                }, info => {
                    if (activePool) {
                        info.projectileObject.SetActive(true);
                    }
                    else {
                        CreateContent(info);
                        info.projectileObject.SetActive(true);
                    }
                }, info => {
                    if (activePool) {
                        info.projectileObject.SetActive(false);
                    }
                    else {
                        info.projectileObject.SetActive(false);
                        DestroyContent(info);
                    }
                },
                DestroyContent, true, _poolDefaultCapacity, _poolMaxCapacity);
            return pool;
        }
    }
}