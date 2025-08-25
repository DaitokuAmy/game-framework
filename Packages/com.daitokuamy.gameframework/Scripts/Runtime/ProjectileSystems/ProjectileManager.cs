using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using GameFramework.Core;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体管理クラス
    /// </summary>
    public class ProjectileManager : DisposableLateUpdatableTask {
        /// <summary>
        /// 更新モード
        /// </summary>
        public enum UpdateMode {
            Update,
            LateUpdate,
        }

        /// <summary>
        /// プール用Objectの情報
        /// </summary>
        private class BulletProjectileInfo {
            public GameObject Prefab;
            public IBulletProjectile Projectile;
        }

        /// <summary>
        /// プール用Objectの情報
        /// </summary>
        private class BeamProjectileInfo {
            public GameObject Prefab;
            public IBeamProjectile Projectile;
        }

        private readonly int _poolDefaultCapacity;
        private readonly int _poolMaxCapacity;
        private readonly UpdateMode _updateMode;
        private readonly ProjectilePlayer _projectilePlayer;
        private readonly Dictionary<GameObject, ObjectPool<BulletProjectileInfo>> _bulletPools = new();
        private readonly Dictionary<GameObject, ObjectPool<BeamProjectileInfo>> _beamPools = new();

        private Transform _rootTransform;
        private bool _activePool = true;
        private GameObject _nullTemplate;

        /// <summary>デフォルト指定のLayer</summary>
        public int DefaultLayer { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="updateMode">更新モード</param>
        /// <param name="poolDefaultCapacity">Poolのデフォルトキャパシティ</param>
        /// <param name="poolMaxCapacity">Poolの最大キャパシティ</param>
        public ProjectileManager(UpdateMode updateMode = UpdateMode.Update, int poolDefaultCapacity = 10, int poolMaxCapacity = 10000) {
            _updateMode = updateMode;
            _poolDefaultCapacity = poolDefaultCapacity;
            _poolMaxCapacity = poolMaxCapacity;
            _projectilePlayer = new ProjectilePlayer();

            // Rootの生成
            var root = new GameObject(nameof(ProjectileManager), typeof(ProjectileManagerDispatcher));
            var dispatcher = root.GetComponent<ProjectileManagerDispatcher>();
            dispatcher.Setup(this);
            Object.DontDestroyOnLoad(root);
            _rootTransform = root.transform;

            // NullTemplateの生成
            _nullTemplate = new GameObject("Null");
            _nullTemplate.transform.SetParent(_rootTransform, false);
            _nullTemplate.SetActive(false);
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
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectileController">飛翔アルゴリズム</param>
        /// <param name="scale">拡大率</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="layer">指定するレイヤー</param>
        public ProjectilePlayer.Handle Play(GameObject prefab, IBulletProjectileController projectileController, Vector3 scale, LayeredTime layeredTime = null, int layer = -1) {
            if (prefab == null) {
                prefab = _nullTemplate;
            }

            // Poolの初期化
            if (!_bulletPools.TryGetValue(prefab, out var pool)) {
                pool = CreateBulletProjectilePool(prefab, _activePool);
                _bulletPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var objectInfo = pool.Get();
            var projectileObj = objectInfo.Projectile;
            projectileObj.SetLocalScale(scale);

            // Layer設定
            if (layer < 0) {
                layer = DefaultLayer;
            }

            SetLayer(projectileObj.transform, layer);

            void Stopped(IBulletProjectile projectile) {
                // Poolに返却
                pool.Release(objectInfo);
            }

            // Projectileを再生
            return _projectilePlayer.Play(projectileObj, projectileController, layeredTime, Stopped);
        }

        /// <summary>
        /// 弾オブジェクトの再生
        /// </summary>
        /// <param name="prefab">再生の実体に使うPrefab</param>
        /// <param name="projectileController">飛翔アルゴリズム</param>
        /// <param name="scale">拡大率</param>
        /// <param name="layeredTime">時間単位</param>
        /// <param name="layer">レイヤー指定</param>
        public ProjectilePlayer.Handle Play(GameObject prefab, IBeamProjectileController projectileController, Vector3 scale, LayeredTime layeredTime = null, int layer = -1) {
            if (prefab == null) {
                prefab = _nullTemplate;
            }

            // Poolの初期化
            if (!_beamPools.TryGetValue(prefab, out var pool)) {
                pool = CreateBeamProjectilePool(prefab, _activePool);
                _beamPools[prefab] = pool;
            }

            // インスタンスの取得、初期化
            var projectileInfo = pool.Get();
            var projectile = projectileInfo.Projectile;
            projectile.SetLocalScale(scale);

            // Layerの設定
            if (layer < 0) {
                layer = DefaultLayer;
            }

            SetLayer(projectile.transform, layer);

            void Stopped(IBeamProjectile _) {
                // Poolに返却
                pool.Release(projectileInfo);
            }

            // Projectileを再生
            return _projectilePlayer.Play(projectile, projectileController, layeredTime, Stopped);
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
            // 全部停止
            StopAll(true);

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
                _projectilePlayer.Update();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            if (_updateMode == UpdateMode.LateUpdate) {
                _projectilePlayer.Update();
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _projectilePlayer.Dispose();

            // Poolを削除
            foreach (var pool in _bulletPools.Values) {
                pool.Dispose();
            }

            foreach (var pool in _beamPools.Values) {
                pool.Dispose();
            }

            _bulletPools.Clear();
            _beamPools.Clear();

            if (_nullTemplate != null) {
                Object.Destroy(_nullTemplate);
                _nullTemplate = null;
            }

            if (_rootTransform != null) {
                Object.Destroy(_rootTransform.gameObject);
                _rootTransform = null;
            }
        }

        /// <summary>
        /// BulletPoolの生成
        /// </summary>
        private ObjectPool<BulletProjectileInfo> CreateBulletProjectilePool(GameObject prefab, bool activePool) {
            // インスタンス生成処理
            void CreateContent(BulletProjectileInfo objectInfo) {
                var gameObj = Object.Instantiate(objectInfo.Prefab, _rootTransform);
                var instance = gameObj.GetComponent<IBulletProjectile>();
                if (instance == null) {
                    instance = gameObj.AddComponent<BulletProjectile>();
                }

                instance.SetActive(false);
                objectInfo.Projectile = instance;
            }

            // 中身の削除
            void DestroyContent(BulletProjectileInfo objectInfo) {
                if (objectInfo == null || objectInfo.Projectile == null) {
                    return;
                }

                if (objectInfo.Projectile != null) {
                    objectInfo.Projectile.Dispose();
                }

                objectInfo.Projectile = null;
            }

            var pool = new ObjectPool<BulletProjectileInfo>(() => {
                    var objectInfo = new BulletProjectileInfo();
                    objectInfo.Prefab = prefab;

                    if (activePool) {
                        CreateContent(objectInfo);
                    }

                    return objectInfo;
                }, info => {
                    if (activePool) {
                        info.Projectile.SetActive(true);
                    }
                    else {
                        CreateContent(info);
                        info.Projectile.SetActive(true);
                    }
                }, info => {
                    if (activePool) {
                        info.Projectile.SetActive(false);
                    }
                    else {
                        info.Projectile.SetActive(false);
                        DestroyContent(info);
                    }
                },
                DestroyContent, true, _poolDefaultCapacity, _poolMaxCapacity);
            return pool;
        }

        /// <summary>
        /// BeamPoolの生成
        /// </summary>
        private ObjectPool<BeamProjectileInfo> CreateBeamProjectilePool(GameObject prefab, bool activePool) {
            // インスタンス生成処理
            void CreateContent(BeamProjectileInfo objectInfo) {
                var gameObj = Object.Instantiate(objectInfo.Prefab, _rootTransform);
                var instance = gameObj.GetComponent<IBeamProjectile>();
                if (instance == null) {
                    instance = gameObj.AddComponent<BeamProjectile>();
                }

                instance.SetActive(false);
                objectInfo.Projectile = instance;
            }

            // 中身の削除
            void DestroyContent(BeamProjectileInfo objectInfo) {
                if (objectInfo == null || objectInfo.Projectile == null) {
                    return;
                }

                if (objectInfo.Projectile != null) {
                    objectInfo.Projectile.Dispose();
                }

                objectInfo.Projectile = null;
            }

            var pool = new ObjectPool<BeamProjectileInfo>(() => {
                    var objectInfo = new BeamProjectileInfo();
                    objectInfo.Prefab = prefab;

                    if (activePool) {
                        CreateContent(objectInfo);
                    }

                    return objectInfo;
                }, info => {
                    if (activePool) {
                        info.Projectile.SetActive(true);
                    }
                    else {
                        CreateContent(info);
                        info.Projectile.SetActive(true);
                    }
                }, info => {
                    if (activePool) {
                        info.Projectile.SetActive(false);
                    }
                    else {
                        info.Projectile.SetActive(false);
                        DestroyContent(info);
                    }
                },
                DestroyContent, true, _poolDefaultCapacity, _poolMaxCapacity);
            return pool;
        }

        /// <summary>
        /// Layerの再帰的な設定
        /// </summary>
        private void SetLayer(Transform trans, int layer) {
            if (trans == null) {
                return;
            }

            trans.gameObject.layer = layer;
            foreach (Transform child in trans) {
                SetLayer(child, layer);
            }
        }
    }
}