using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// プール管理用アセットストレージ
    /// </summary>
    public class PoolSceneAssetStorage : SceneAssetStorage {
        // キャッシュキー
        private readonly struct CacheKey {
            public readonly string Address;
            public readonly LoadSceneMode Mode;

            public CacheKey(string address, LoadSceneMode mode) {
                Address = address;
                Mode = mode;
            }
        }

        // キャッシュ情報
        private class CacheInfo {
            public SceneAssetHandle handle;
        }

        // キャッシュ
        private readonly Dictionary<CacheKey, CacheInfo> _cacheInfos = new();
        // 読み込み順番管理
        private readonly List<CacheKey> _fetchKeys = new();

        // 同時キャッシュ数
        private int _amount = 3;
        public int Amount {
            get => _amount;
            set {
                _amount = Mathf.Max(value, 0);
                FetchAddress();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        /// <param name="amount">同時キャッシュ数</param>
        public PoolSceneAssetStorage(AssetManager assetManager, int amount = 3) : base(assetManager) {
            Amount = amount;
        }

        /// <inheritdoc/>
        public override void Dispose() {
            UnloadAssets();
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        public SceneAssetHandle LoadAssetAsync(SceneAssetRequest request) {
            if (Amount == 0) {
                return LoadAssetAsyncInternal(request);
            }

            var cacheKey = new CacheKey(request.Address, request.Mode);

            // 既にキャッシュがある場合、キャッシュ経由で読み込みを待つ
            if (_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                FetchAddress(cacheKey);
                return cacheInfo.handle.Acquire();
            }

            // キャッシュがない場合、LoadingStatusObserverを使って読み込み
            cacheInfo = new CacheInfo();
            cacheInfo.handle = LoadAssetAsyncInternal(request);
            if (!cacheInfo.handle.IsValid) {
                return cacheInfo.handle;
            }

            _cacheInfos[cacheKey] = cacheInfo;
            FetchAddress(cacheKey);
            return cacheInfo.handle.Acquire();
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public Scene GetAsset(string address) {
            var matches = _cacheInfos.Where(pair => pair.Key.Address == address).ToArray();
            if (matches.Length > 1) {
                Debug.unityLogger.LogError(GetType().Name, $"Multiple cache infos were found. Specify request with mode. {address}");
                return new Scene();
            }

            foreach (var pair in matches) {
                return pair.Value.handle.Scene;
            }

            return new Scene();
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public Scene GetAsset(SceneAssetRequest request) {
            var cacheKey = new CacheKey(request.Address, request.Mode);
            if (!_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                return new Scene();
            }

            return cacheInfo.handle.Scene;
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void UnloadAsset(SceneAssetRequest request) {
            RemoveCache(new CacheKey(request.Address, request.Mode));
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void UnloadAssets() {
            var keys = _cacheInfos.Keys.ToArray();

            foreach (var key in keys) {
                RemoveCache(key);
            }
        }

        /// <summary>
        /// アドレスのフェッチ（最大数を超えたアセットは自動でアンロード）
        /// </summary>
        private void FetchAddress(CacheKey? cacheKey = null) {
            if (cacheKey.HasValue) {
                _fetchKeys.Remove(cacheKey.Value);
                _fetchKeys.Add(cacheKey.Value);
            }

            while (_fetchKeys.Count > Amount) {
                // 古い物は削除
                RemoveCache(_fetchKeys[0]);
            }
        }

        /// <summary>
        /// キャッシュの解放
        /// </summary>
        private void RemoveCache(CacheKey cacheKey) {
            if (!_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                Debug.unityLogger.LogError(GetType().Name, $"Not found cache info. {cacheKey.Address}");
                return;
            }

            if (cacheInfo.handle.IsValid) {
                cacheInfo.handle.Release();
            }

            _cacheInfos.Remove(cacheKey);
            _fetchKeys.Remove(cacheKey);
        }
    }
}
