using UnityEngine;
using GameFramework.Pooling;

namespace GameFramework.ProjectileSystem {
    /// <summary>
    /// 飛翔体管理クラス
    /// </summary>
    public sealed class ProjectileManager : DisposableLateUpdatable {
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
        private readonly ProjectilePlayer _projectilePlayer;
        private readonly KeyedObjectPool<GameObject, BulletProjectileInfo> _bulletPool;
        private readonly KeyedObjectPool<GameObject, BeamProjectileInfo> _beamPool;

        private Transform _rootTransform;
        private bool _activePool = true;
        private GameObject _nullTemplate;

        /// <summary>デフォルト指定のLayer</summary>
        public int DefaultLayer { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="poolDefaultCapacity">Poolのデフォルトキャパシティ</param>
        /// <param name="poolMaxCapacity">Poolの最大キャパシティ</param>
        public ProjectileManager(int poolDefaultCapacity = 10, int poolMaxCapacity = 10000) {
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

            // Pool構築
            _bulletPool = new KeyedObjectPool<GameObject, BulletProjectileInfo>(prefab => {
                var pool = new ObjectPool<BulletProjectileInfo, GameObject>(prefab, pfb => {
                    var info = new BulletProjectileInfo();
                    info.Prefab = pfb;
                    var gameObj = Object.Instantiate(info.Prefab, _rootTransform);
                    var instance = gameObj.GetComponent<IBulletProjectile>();
                    if (instance == null) {
                        instance = gameObj.AddComponent<BulletProjectile>();
                    }

                    instance.SetActive(false);
                    info.Projectile = instance;
                    return info;
                }, (_, info) => {
                    info.Projectile.SetActive(true);
                }, (_, info) => {
                    info.Projectile.SetActive(false);
                }, (_, info) => {
                    if (info?.Projectile == null) {
                        return;
                    }

                    info.Projectile?.Dispose();

                    info.Projectile = null;
                }, true, _poolDefaultCapacity, _poolMaxCapacity);
                return pool;
            });

            _beamPool = new KeyedObjectPool<GameObject, BeamProjectileInfo>(prefab => {
                var pool = new ObjectPool<BeamProjectileInfo, GameObject>(prefab, pfb => {
                    var info = new BeamProjectileInfo();
                    info.Prefab = pfb;
                    var gameObj = Object.Instantiate(info.Prefab, _rootTransform);
                    var instance = gameObj.GetComponent<IBeamProjectile>();
                    if (instance == null) {
                        instance = gameObj.AddComponent<BeamProjectile>();
                    }

                    instance.SetActive(false);
                    info.Projectile = instance;
                    return info;
                }, (_, info) => {
                    info.Projectile.SetActive(true);
                }, (_, info) => {
                    info.Projectile.SetActive(false);
                }, (_, info) => {
                    if (info?.Projectile == null) {
                        return;
                    }

                    info.Projectile?.Dispose();

                    info.Projectile = null;
                }, true, _poolDefaultCapacity, _poolMaxCapacity);
                return pool;
            });
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
            _bulletPool.SetPoolingEnabled(_activePool, true);
            _beamPool.SetPoolingEnabled(_activePool, true);
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

            // インスタンスの取得、初期化
            var projectileInfo = _bulletPool.Get(prefab);
            var projectile = projectileInfo.Projectile;
            projectile.SetLocalScale(scale);

            // Layer設定
            if (layer < 0) {
                layer = DefaultLayer;
            }

            SetLayer(projectile.transform, layer);

            void Stopped(IBulletProjectile _) {
                // Poolに返却
                _bulletPool.Release(prefab, projectileInfo);
            }

            // Projectileを再生
            return _projectilePlayer.Play(projectile, projectileController, layeredTime, Stopped);
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

            // インスタンスの取得、初期化
            var projectileInfo = _beamPool.Get(prefab);
            var projectile = projectileInfo.Projectile;
            projectile.SetLocalScale(scale);

            // Layerの設定
            if (layer < 0) {
                layer = DefaultLayer;
            }

            SetLayer(projectile.transform, layer);

            void Stopped(IBeamProjectile _) {
                // Poolに返却
                _beamPool.Release(prefab, projectileInfo);
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
            _bulletPool.ClearAll();
            _beamPool.ClearAll();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            _projectilePlayer.Update();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            _projectilePlayer.Dispose();

            // Poolを削除
            _bulletPool.ClearAll();
            _beamPool.ClearAll();

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