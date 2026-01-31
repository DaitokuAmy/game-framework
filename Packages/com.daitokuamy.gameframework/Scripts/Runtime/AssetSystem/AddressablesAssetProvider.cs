#if USE_ADDRESSABLES

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// Addressablesを使ったアセット提供用クラス
    /// </summary>
    public sealed class AddressablesAssetProvider : IAssetProvider {
        /// <summary>
        /// アセット情報
        /// </summary>
        private class AssetInfo<T> : IAssetInfo<T>
            where T : Object {
            private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<T> _handle;

            /// <inheritdoc/>
            bool IAssetInfo<T>.IsDone => _handle.IsDone;
            /// <inheritdoc/>
            T IAssetInfo<T>.Asset => _handle.Result;
            /// <inheritdoc/>
            Exception IAssetInfo<T>.Exception => _handle.OperationException;

            public AssetInfo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<T> handle) {
                _handle = handle;
            }

            /// <inheritdoc/>
            public void Dispose() {
                if (!_handle.IsValid()) {
                    return;
                }

                Addressables.Release(_handle);
            }
        }

        /// <summary>
        /// シーンアセット情報
        /// </summary>
        private class SceneAssetInfo : ISceneAssetInfo {
            private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<SceneInstance> _handle;

            /// <inheritdoc/>
            bool ISceneAssetInfo.IsDone => _handle.IsDone;
            /// <inheritdoc/>
            Scene ISceneAssetInfo.Scene => _handle.Result.Scene;
            /// <inheritdoc/>
            Exception ISceneAssetInfo.Exception => _handle.OperationException;

            public SceneAssetInfo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<SceneInstance> handle) {
                _handle = handle;
            }

            /// <inheritdoc/>
            public void Dispose() {
                if (!_handle.IsValid()) {
                    return;
                }

                Addressables.Release(_handle);
            }

            /// <inheritdoc/>
            AsyncOperation ISceneAssetInfo.ActivateAsync() {
                if (!_handle.IsValid()) {
                    return null;
                }

                return _handle.Result.ActivateAsync();
            }
        }

        /// <inheritdoc/>
        AssetHandle<T> IAssetProvider.LoadAsync<T>(string address) {
            var operationHandle = Addressables.LoadAssetAsync<T>(address);
            var assetInfo = new AssetInfo<T>(operationHandle);
            return new AssetHandle<T>(assetInfo);
        }

        /// <inheritdoc/>
        bool IAssetProvider.Contains<T>(string address) {
            foreach (var locator in Addressables.ResourceLocators) {
                if (locator is ResourceLocationMap map) {
                    if (map.Locations.ContainsKey(address)) {
                        return true;
                    }
                }
                else {
                    if (locator.Locate(address, typeof(T), out var _)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc/>
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address, LoadSceneMode mode) {
            var operationHandle = Addressables.LoadSceneAsync(address, mode, false);
            var assetInfo = new SceneAssetInfo(operationHandle);
            return new SceneAssetHandle(assetInfo);
        }

        /// <inheritdoc/>
        bool IAssetProvider.ContainsScene(string address) {
            foreach (var locator in Addressables.ResourceLocators) {
                if (locator is ResourceLocationMap map) {
                    if (map.Locations.ContainsKey(address)) {
                        return true;
                    }
                }
                else {
                    if (locator.Locate(address, typeof(SceneInstance), out var _)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

#endif