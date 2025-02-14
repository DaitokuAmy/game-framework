#if USE_ADDRESSABLES

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// Addressablesを使ったアセット提供用クラス
    /// </summary>
    public sealed class AddressablesAssetProvider : IAssetProvider {
        /// <summary>
        /// アセット情報
        /// </summary>
        private class AssetInfo<T> : IAssetInfo<T>
            where T : Object {
            private AsyncOperationHandle<T> _handle;

            bool IAssetInfo<T>.IsDone => _handle.IsDone;
            T IAssetInfo<T>.Asset => _handle.Result;
            Exception IAssetInfo<T>.Exception => _handle.OperationException;

            public AssetInfo(AsyncOperationHandle<T> handle) {
                _handle = handle;
            }

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
            private AsyncOperationHandle<SceneInstance> _handle;

            bool ISceneAssetInfo.IsDone => _handle.IsDone;
            Scene ISceneAssetInfo.Scene => _handle.Result.Scene;
            Exception ISceneAssetInfo.Exception => _handle.OperationException;

            public SceneAssetInfo(AsyncOperationHandle<SceneInstance> handle) {
                _handle = handle;
            }

            public void Dispose() {
                if (!_handle.IsValid()) {
                    return;
                }

                Addressables.Release(_handle);
            }
            
            AsyncOperation ISceneAssetInfo.ActivateAsync() {
                if (!_handle.IsValid()) {
                    return null;
                }

                return _handle.Result.ActivateAsync();
            }
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        AssetHandle<T> IAssetProvider.LoadAsync<T>(string address) {
            var operationHandle = Addressables.LoadAssetAsync<T>(address);
            var assetInfo = new AssetInfo<T>(operationHandle);
            return new AssetHandle<T>(assetInfo);
        }

        /// <summary>
        /// アセットが含まれているか
        /// </summary>
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

        /// <summary>
        /// シーンアセットの読み込み
        /// </summary>
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address, LoadSceneMode mode) {
            var operationHandle = Addressables.LoadSceneAsync(address, mode, false);
            var assetInfo = new SceneAssetInfo(operationHandle);
            return new SceneAssetHandle(assetInfo);
        }

        /// <summary>
        /// シーンアセットが含まれているか
        /// </summary>
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