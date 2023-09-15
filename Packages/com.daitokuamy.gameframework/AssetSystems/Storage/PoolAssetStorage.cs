using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// プール管理用アセットストレージ
    /// </summary>
    public class PoolAssetStorage<TAsset> : AssetStorage
        where TAsset : Object {
        // キャッシュ情報
        private class CacheInfo {
            public AssetHandle<TAsset> handle;
        }

        // キャッシュ
        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();
        // 読み込み順番管理
        private readonly List<string> _fetchAddresses = new();

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
        public PoolAssetStorage(AssetManager assetManager, int amount = 3) : base(assetManager) {
            Amount = amount;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public override void Dispose() {
            UnloadAssets();
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        public AssetHandle<TAsset> LoadAssetAsync(AssetRequest<TAsset> request) {
            var address = request.Address;

            // Addressのフェッチ
            FetchAddress(address);

            // 既にキャッシュがある場合、キャッシュ経由で読み込みを待つ
            if (_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                return cacheInfo.handle;
            }

            // キャッシュがない場合、読み込んでキャッシュ管理
            cacheInfo = new CacheInfo();
            cacheInfo.handle = LoadAssetAsyncInternal(request);
            _cacheInfos[address] = cacheInfo;
            return cacheInfo.handle;
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public TAsset GetAsset(string address) {
            if (!_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                return null;
            }

            return cacheInfo.handle.Asset;
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public TAsset GetAsset(AssetRequest<TAsset> request) {
            return GetAsset(request.Address);
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
        private void FetchAddress(string address = null) {
            if (!string.IsNullOrEmpty(address)) {
                _fetchAddresses.Remove(address);
                _fetchAddresses.Add(address);
            }

            while (_fetchAddresses.Count > Amount) {
                // 古い物は削除
                RemoveCache(_fetchAddresses[0]);
            }
        }

        /// <summary>
        /// キャッシュの解放
        /// </summary>
        private void RemoveCache(string address) {
            if (!_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                Debug.unityLogger.LogError(GetType().Name, $"Not found cache info. {address}");
                return;
            }

            if (cacheInfo.handle.IsValid) {
                cacheInfo.handle.Release();
            }

            _cacheInfos.Remove(address);
            _fetchAddresses.Remove(address);
        }
    }
}