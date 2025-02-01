using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// シンプルなアンロード管理を持つだけのアセットストレージ
    /// </summary>
    public class SimpleAssetStorage<TAsset> : AssetStorage
        where TAsset : Object {
        // キャッシュ情報
        private class CacheInfo {
            public AssetHandle<TAsset> handle;
        }

        // キャッシュ
        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        public SimpleAssetStorage(AssetManager assetManager) : base(assetManager) {
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
        /// 解放処理
        /// </summary>
        public void UnloadAsset(string address) {
            if (!_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                Debug.unityLogger.LogError(GetType().Name, $"Not found cache info. {address}");
                return;
            }

            if (cacheInfo.handle.IsValid) {
                cacheInfo.handle.Release();
            }

            _cacheInfos.Remove(address);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void UnloadAssets() {
            var keys = _cacheInfos.Keys.ToArray();

            foreach (var pair in _cacheInfos) {
                var cacheInfo = pair.Value;
                if (cacheInfo.handle.IsValid) {
                    cacheInfo.handle.Release();
                }
            }
            
            _cacheInfos.Clear();
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
    }
}