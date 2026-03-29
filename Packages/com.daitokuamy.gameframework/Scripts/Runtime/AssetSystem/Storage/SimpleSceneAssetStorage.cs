using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// シンプルなアンロード管理を持つだけのシーンアセットストレージ
    /// </summary>
    public class SimpleSceneAssetStorage : SceneAssetStorage {
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        public SimpleSceneAssetStorage(AssetManager assetManager) : base(assetManager) {
        }

        /// <inheritdoc/>
        public override void Dispose() {
            UnloadAssets();
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        public SceneAssetHandle LoadAssetAsync(SceneAssetRequest request) {
            var cacheKey = new CacheKey(request.Address, request.Mode);

            // 既にキャッシュがある場合、キャッシュ経由で読み込みを待つ
            if (_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                return cacheInfo.handle.Acquire();
            }

            // キャッシュがない場合、読み込んでキャッシュ管理
            cacheInfo = new CacheInfo();
            cacheInfo.handle = LoadAssetAsyncInternal(request);
            if (!cacheInfo.handle.IsValid) {
                return cacheInfo.handle;
            }

            _cacheInfos[cacheKey] = cacheInfo;
            return cacheInfo.handle.Acquire();
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void UnloadAsset(string address) {
            var matches = _cacheInfos.Keys.Where(key => key.Address == address).ToArray();
            if (matches.Length > 1) {
                Debug.unityLogger.LogError(GetType().Name, $"Multiple cache infos were found. Specify request with mode. {address}");
                return;
            }

            var removed = false;
            foreach (var key in matches) {
                var cacheInfo = _cacheInfos[key];
                if (cacheInfo.handle.IsValid) {
                    cacheInfo.handle.Release();
                }

                _cacheInfos.Remove(key);
                removed = true;
            }

            if (!removed) {
                Debug.unityLogger.LogError(GetType().Name, $"Not found cache info. {address}");
            }
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void UnloadAsset(SceneAssetRequest request) {
            var cacheKey = new CacheKey(request.Address, request.Mode);
            if (!_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                Debug.unityLogger.LogError(GetType().Name, $"Not found cache info. {request.Address}");
                return;
            }

            if (cacheInfo.handle.IsValid) {
                cacheInfo.handle.Release();
            }

            _cacheInfos.Remove(cacheKey);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void UnloadAssets() {
            foreach (var pair in _cacheInfos) {
                var cacheInfo = pair.Value;
                if (cacheInfo.handle.IsValid) {
                    cacheInfo.handle.Release();
                }
            }
            
            _cacheInfos.Clear();
        }

        /// <summary>
        /// 読み込み済みシーンの取得
        /// </summary>
        public Scene GetAsset(string address) {
            var matches = _cacheInfos.Where(pair => pair.Key.Address == address).ToArray();
            if (matches.Length > 1) {
                Debug.unityLogger.LogError(GetType().Name, $"Multiple cache infos were found. Specify request with mode. {address}");
                return default;
            }

            foreach (var pair in matches) {
                return pair.Value.handle.Scene;
            }

            return default;
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public Scene GetAsset(SceneAssetRequest request) {
            var cacheKey = new CacheKey(request.Address, request.Mode);
            if (!_cacheInfos.TryGetValue(cacheKey, out var cacheInfo)) {
                return default;
            }

            return cacheInfo.handle.Scene;
        }
    }
}
