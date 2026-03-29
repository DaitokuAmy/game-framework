#if USE_ADDRESSABLES

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// Addressablesを使ったアセット提供用クラス
    /// </summary>
    public sealed class AddressablesAssetProvider : IAssetProvider {
        /// <summary>Providerキー</summary>
        public const string ProviderKey = "Addressables";

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
            private bool _isDisposed;
            private bool _isUnloadStarted;

            /// <inheritdoc/>
            bool ISceneAssetInfo.IsDone => _handle.IsDone;
            /// <inheritdoc/>
            Scene ISceneAssetInfo.Scene => _handle.Result.Scene;
            /// <inheritdoc/>
            Exception ISceneAssetInfo.Exception => _handle.OperationException;

            public SceneAssetInfo(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<SceneInstance> handle) {
                _handle = handle;
                _handle.Completed += OnCompleted;
            }

            /// <inheritdoc/>
            public void Dispose() {
                _isDisposed = true;
                TryReleaseOrUnload();
            }

            /// <inheritdoc/>
            AsyncOperation ISceneAssetInfo.ActivateAsync() {
                if (!_handle.IsValid()) {
                    return null;
                }

                return _handle.Result.ActivateAsync();
            }

            private void OnCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<SceneInstance> handle) {
                if (_isDisposed) {
                    TryReleaseOrUnload();
                }
            }

            private void TryReleaseOrUnload() {
                if (_isUnloadStarted || !_handle.IsValid() || !_handle.IsDone) {
                    return;
                }

                _isUnloadStarted = true;
                if (_handle.Status == AsyncOperationStatus.Succeeded) {
                    Addressables.UnloadSceneAsync(_handle, true);
                }
                else {
                    Addressables.Release(_handle);
                }
            }
        }

        /// <inheritdoc/>
        string IAssetProvider.Key => ProviderKey;

        /// <inheritdoc/>
        AssetHandle<T> IAssetProvider.LoadAsync<T>(string address) {
            var operationHandle = Addressables.LoadAssetAsync<T>(address);
            var assetInfo = new AssetInfo<T>(operationHandle);
            return new AssetHandle<T>(assetInfo);
        }

        /// <inheritdoc/>
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address, LoadSceneMode mode) {
            var operationHandle = Addressables.LoadSceneAsync(address, mode, false);
            var assetInfo = new SceneAssetInfo(operationHandle);
            return new SceneAssetHandle(assetInfo);
        }
    }
}

#endif
