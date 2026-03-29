#if USE_ADDRESSABLES

using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// Addressables用アセットローダー
    /// </summary>
    public sealed class AddressablesAssetLoader : IAssetLoader {
        /// <summary>
        /// アセット読み込みハンドル
        /// </summary>
        private sealed class AssetLoadHandle<TAsset> : IAssetLoadHandle<TAsset>
            where TAsset : Object {
            private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TAsset> _handle;
            private bool _isReleased;

            /// <inheritdoc/>
            public bool IsDone => _isReleased || !_handle.IsValid() || _handle.IsDone;
            /// <inheritdoc/>
            public TAsset Asset => _handle.IsValid() && _handle.Status == AsyncOperationStatus.Succeeded
                ? _handle.Result
                : null;
            /// <inheritdoc/>
            public Exception Exception => _handle.IsValid() ? _handle.OperationException : null;
            /// <inheritdoc/>
            public bool IsValid => !_isReleased && _handle.IsValid();
            /// <inheritdoc/>
            TAsset IProcess<TAsset>.Result => Asset;
            /// <inheritdoc/>
            object IEnumerator.Current => null;

            /// <summary>
            /// コンストラクター
            /// </summary>
            public AssetLoadHandle(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TAsset> handle) {
                _handle = handle;
            }

            /// <inheritdoc/>
            public void Release() {
                if (_isReleased) {
                    return;
                }

                _isReleased = true;
                if (_handle.IsValid()) {
                    Addressables.Release(_handle);
                }
            }

            /// <inheritdoc/>
            public void Dispose() {
                Release();
            }

            /// <inheritdoc/>
            bool IEnumerator.MoveNext() {
                return !IsDone;
            }

            /// <inheritdoc/>
            void IEnumerator.Reset() {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public bool CanLoad<TAsset>(IAssetRequest<TAsset> request)
            where TAsset : Object {
            if (!request.IsValid) {
                return false;
            }

            foreach (var locator in Addressables.ResourceLocators) {
                if (locator.Locate(request.Address, typeof(TAsset), out _)) {
                    return true;
                }

                if (locator.Locate(request.Address, typeof(object), out _)) {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public IAssetLoadHandle<TAsset> LoadAsync<TAsset>(IAssetRequest<TAsset> request)
            where TAsset : Object {
            var operationHandle = Addressables.LoadAssetAsync<TAsset>(request.Address);
            return new AssetLoadHandle<TAsset>(operationHandle);
        }
    }
}

#endif
