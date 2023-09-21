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
            private readonly ObjectPool<IBulletProjectileObject> _pool;
            private readonly IBulletProjectileObject _projectileObject;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public BulletPlayingInfo(ObjectPool<IBulletProjectileObject> pool, IBulletProjectileObject projectileObject, LayeredTime layeredTime, int layer)
                : base(layeredTime) {
                _pool = pool;
                _projectileObject = projectileObject;
                SetLayer(projectileObject.transform, layer);
            }

            /// <summary>
            /// TimeScaleの変更通知
            /// </summary>
            protected override void OnChangedTimeScale(float timeScale) {
                _projectileObject.SetSpeed(timeScale);
            }

            /// <summary>
            /// Poolへの返却
            /// </summary>
            protected override void ReleaseInternal() {
                _pool.Release(_projectileObject);
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                _projectileObject.Update(deltaTime);
                return _projectileObject.IsPlaying;
            }
        }

        /// <summary>
        /// 再生中情報
        /// </summary>
        private class BeamPlayingInfo : PlayingInfo {
            private readonly ObjectPool<IBeamProjectileObject> _pool;
            private readonly IBeamProjectileObject _projectileObject;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public BeamPlayingInfo(ObjectPool<IBeamProjectileObject> pool, IBeamProjectileObject projectileObject, LayeredTime layeredTime, int layer)
                : base(layeredTime) {
                _pool = pool;
                _projectileObject = projectileObject;
                SetLayer(projectileObject.transform, layer);
            }

            /// <summary>
            /// TimeScaleの変更通知
            /// </summary>
            protected override void OnChangedTimeScale(float timeScale) {
                _projectileObject.SetSpeed(timeScale);
            }

            /// <summary>
            /// Poolへの返却
            /// </summary>
            protected override void ReleaseInternal() {
                _pool.Release(_projectileObject);
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            protected override bool UpdateInternal(float deltaTime) {
                _projectileObject.Update(deltaTime);
                return _projectileObject.IsPlaying;
            }
        }

        // コリジョンマネージャ
        private CollisionManager _collisionManager;
        // 更新モード
        private UpdateMode _updateMode;

        // 管理ルートになるTransform
        private Transform _rootTransform;
        // Projectile再生用Player
        private ProjectilePlayer _projectilePlayer;
        // ProjectileObjectPool管理用
        private Dictionary<GameObject, ObjectPool<IBulletProjectileObject>> _bulletPools = new();
        private Dictionary<GameObject, ObjectPool<IBeamProjectileObject>> _beamPools = new();
        // 再生中情報
        private List<PlayingInfo> _playingInfos = new();

        /// <summary>デフォルト指定のLayer</summary>
        public int DefaultLayer { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProjectileObjectManager(CollisionManager collisionManager, UpdateMode updateMode = UpdateMode.Update) {
            _collisionManager = collisionManager;
            _updateMode = updateMode;
            _projectilePlayer = new ProjectilePlayer();

            // Rootの生成
            var root = new GameObject("ProjectileObjectManager", typeof(ProjectileObjectManagerDispatcher));
            var dispatcher = root.GetComponent<ProjectileObjectManagerDispatcher>();
            dispatcher.Setup(this);
            UnityEngine.Object.DontDestroyOnLoad(root);
            _rootTransform = root.transform;
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
                pool = new ObjectPool<IBulletProjectileObject>(
                    () => CreateInstance(prefab), OnGetInstance, OnReleaseInstance, OnDestroyInstance);
                _bulletPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var instance = pool.Get();
            instance.SetLocalScale(scale);
            instance.Start(projectile);

            // Raycastの生成
            var raycastCollision = (IRaycastCollision)(instance.RaycastRadius > float.Epsilon
                ? new SphereRaycastCollision(projectile.Position, projectile.Position, instance.RaycastRadius)
                : new LineRaycastCollision(projectile.Position, projectile.Position));
            var collisionHandle = new CollisionHandle();
            var lastHitPoint = default(Vector3?);

            // Projectileを再生
            var projectileHandle = _projectilePlayer.Play(projectile, layeredTime, prj => {
                instance.UpdateProjectile(prj);
                raycastCollision.March(prj.Position);
                onUpdatedTransform?.Invoke(prj.Position, prj.Rotation);
            }, () => {
                collisionHandle.Dispose();
                if (lastHitPoint != null) {
                    instance.SetPosition(lastHitPoint.Value);
                }

                instance.Exit();
                onExit?.Invoke();
            });

            // コリジョン登録
            collisionHandle = _collisionManager.Register(raycastCollision, hitLayerMask, customData, result => {
                if (checkHitFunc != null && !checkHitFunc.Invoke(result)) {
                    return;
                }

                instance.OnHitCollision(result);
                listener.OnHitRaycastCollision(result);
                if (hitCount >= 0 && result.hitCount >= hitCount) {
                    // 着弾位置に移動してから廃棄する
                    if (result.raycastHit.distance > float.Epsilon) {
                        lastHitPoint = result.raycastHit.point;
                        instance.SetPosition(lastHitPoint.Value);
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
            _playingInfos.Add(new BulletPlayingInfo(pool, instance, layeredTime, layer));

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
                pool = new ObjectPool<IBeamProjectileObject>(
                    () => CreateBeamInstance(prefab), OnGetInstance, OnReleaseInstance, OnDestroyInstance);
                _beamPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var instance = pool.Get();
            instance.SetLocalScale(scale);
            instance.Start(projectile);

            // Raycastの生成
            var raycastCollision = (IRaycastCollision)(instance.RaycastRadius > float.Epsilon
                ? new SphereRaycastCollision(projectile.TailPosition, projectile.HeadPosition, instance.RaycastRadius)
                : new LineRaycastCollision(projectile.TailPosition, projectile.HeadPosition));
            var collisionHandle = new CollisionHandle();

            // Projectileを再生
            var projectileHandle = _projectilePlayer.Play(projectile, layeredTime, prj => {
                instance.UpdateProjectile(prj);
                raycastCollision.Start = prj.TailPosition;
                raycastCollision.End = prj.HeadPosition;
                raycastCollision.IsActive = prj.IsSolid;
                onUpdatedTransform?.Invoke(prj.HeadPosition, prj.TailPosition, prj.Rotation);
            }, () => {
                collisionHandle.Dispose();
                instance.Exit();
                onExit?.Invoke();
            });

            // コリジョン登録
            collisionHandle = _collisionManager.Register(raycastCollision, hitLayerMask, customData, result => {
                if (checkHitFunc != null && !checkHitFunc.Invoke(result)) {
                    return;
                }

                instance.OnHitCollision(result);
                listener.OnHitRaycastCollision(result);
                if (hitCount >= 0 && result.hitCount >= hitCount) {
                    projectileHandle.Stop();
                }
            });

            if (layer < 0) {
                layer = DefaultLayer;
            }

            // 再生情報として登録
            _playingInfos.Add(new BeamPlayingInfo(pool, instance, layeredTime, layer));

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
        /// インスタンス生成処理
        /// </summary>
        private IBulletProjectileObject CreateInstance(GameObject prefab) {
            var gameObj = UnityEngine.Object.Instantiate(prefab, _rootTransform);
            var instance = gameObj.GetComponent<IBulletProjectileObject>();
            if (instance == null) {
                instance = gameObj.AddComponent<BulletProjectileObject>();
            }

            instance.SetActive(false);
            return instance;
        }

        /// <summary>
        /// インスタンス生成処理
        /// </summary>
        private IBeamProjectileObject CreateBeamInstance(GameObject prefab) {
            var gameObj = UnityEngine.Object.Instantiate(prefab, _rootTransform);
            var instance = gameObj.GetComponent<IBeamProjectileObject>();
            if (instance == null) {
                instance = gameObj.AddComponent<BeamProjectileObject>();
            }

            instance.SetActive(false);
            return instance;
        }

        /// <summary>
        /// インスタンス廃棄時処理
        /// </summary>
        private void OnDestroyInstance(IBulletProjectileObject instance) {
            instance.Dispose();
        }

        /// <summary>
        /// インスタンス廃棄時処理
        /// </summary>
        private void OnDestroyInstance(IBeamProjectileObject instance) {
            instance.Dispose();
        }

        /// <summary>
        /// インスタンス取得時処理
        /// </summary>
        private void OnGetInstance(IBulletProjectileObject instance) {
            instance.SetActive(true);
        }

        /// <summary>
        /// インスタンス取得時処理
        /// </summary>
        private void OnGetInstance(IBeamProjectileObject instance) {
            instance.SetActive(true);
        }

        /// <summary>
        /// インスタンス返却時処理
        /// </summary>
        private void OnReleaseInstance(IBulletProjectileObject instance) {
            instance.SetActive(false);
        }

        /// <summary>
        /// インスタンス返却時処理
        /// </summary>
        private void OnReleaseInstance(IBeamProjectileObject instance) {
            instance.SetActive(false);
        }
    }
}