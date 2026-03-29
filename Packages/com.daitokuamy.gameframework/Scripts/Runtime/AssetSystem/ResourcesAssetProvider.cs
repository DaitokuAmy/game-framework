using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// Resourcesを使ったアセット提供用クラス
    /// </summary>
    public sealed class ResourcesAssetProvider : IAssetProvider {
        /// <summary>Providerキー</summary>
        public const string ProviderKey = "Resources";

        /// <summary>
        /// アセット情報
        /// </summary>
        private class AssetInfo<T> : IAssetInfo<T>
            where T : Object {
            private readonly string _address;
            private ResourceRequest _request;

            /// <inheritdoc/>
            bool IAssetInfo<T>.IsDone => _request == null || _request.isDone;
            /// <inheritdoc/>
            T IAssetInfo<T>.Asset => (T)_request?.asset;
            /// <inheritdoc/>
            Exception IAssetInfo<T>.Exception => _request != null && _request.isDone && _request.asset == null
                ? new FileNotFoundException($"Not found asset. [{_address}]")
                : null;

            public AssetInfo(string address, ResourceRequest request) {
                _address = address;
                _request = request;
            }

            /// <inheritdoc/>
            public void Dispose() {
                // Unloadはしない
            }
        }

        /// <summary>
        /// シーンアセット情報
        /// </summary>
        private class SceneAssetInfo : ISceneAssetInfo {
            private Scene _scene;
            
            /// <inheritdoc/>
            bool ISceneAssetInfo.IsDone => true;
            /// <inheritdoc/>
            Scene ISceneAssetInfo.Scene => _scene;
            /// <inheritdoc/>
            Exception ISceneAssetInfo.Exception => new Exception("Not supported scene asset.");

            public SceneAssetInfo() {
                _scene = new Scene();
            }

            /// <inheritdoc/>
            public void Dispose() {
                // Unloadはしない
            }

            /// <inheritdoc/>
            AsyncOperation ISceneAssetInfo.ActivateAsync() {
                return null;
            }
        }

        /// <inheritdoc/>
        string IAssetProvider.Key => ProviderKey;

        /// <inheritdoc/>
        AssetHandle<T> IAssetProvider.LoadAsync<T>(string address) {
            if (string.IsNullOrEmpty(address)) {
                return AssetHandle<T>.Empty;
            }

            var resourcesPath = GetResourcesPath(address);
            // 読み込み開始
            var request = Resources.LoadAsync<T>(resourcesPath);
            var info = new AssetInfo<T>(address, request);
            return new AssetHandle<T>(info);
        }

        /// <inheritdoc/>
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address, LoadSceneMode mode) {
            return SceneAssetHandle.Empty;
        }

        /// <summary>
        /// Resourcesから読み込む際のPathに変換
        /// </summary>
        private string GetResourcesPath(string address) {
            // Resources以下のパスに変換
            var index = address.LastIndexOf("/Resources/", StringComparison.Ordinal);
            if (index >= 0) {
                address = address.Substring(index + "/Resources/".Length);
            }

            // 拡張子を削除
            var extension = Path.GetExtension(address);
            if (!string.IsNullOrEmpty(extension)) {
                address = address.Substring(0, address.Length - extension.Length);
            }

            return address;
        }
    }
}
