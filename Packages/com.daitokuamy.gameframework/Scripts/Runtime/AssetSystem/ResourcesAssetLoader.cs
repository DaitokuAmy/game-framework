using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// Resources用アセットローダー
    /// </summary>
    public sealed class ResourcesAssetLoader : IAssetLoader {
        /// <summary>
        /// アセット読み込みハンドル
        /// </summary>
        private sealed class AssetLoadHandle<TAsset> : IAssetLoadHandle<TAsset>
            where TAsset : Object {
            private readonly string _address;
            private readonly ResourceRequest _request;
            private readonly Exception _exception;
            private bool _isReleased;

            /// <inheritdoc/>
            public bool IsDone => _isReleased || _request == null || _request.isDone;
            /// <inheritdoc/>
            public TAsset Asset => _isReleased || _request == null || !_request.isDone
                ? null
                : _request.asset as TAsset;
            /// <inheritdoc/>
            public Exception Exception => _exception ?? (_request != null && _request.isDone && _request.asset == null
                ? new FileNotFoundException($"Not found asset. [{_address}]")
                : null);
            /// <inheritdoc/>
            public bool IsValid => !_isReleased && _request != null;
            /// <inheritdoc/>
            TAsset IProcess<TAsset>.Result => Asset;
            /// <inheritdoc/>
            object IEnumerator.Current => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public AssetLoadHandle(string address, ResourceRequest request) {
                _address = address;
                _request = request;
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public AssetLoadHandle(Exception exception) {
                _exception = exception;
            }

            /// <inheritdoc/>
            public void Release() {
                _isReleased = true;
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

            return Resources.Load(GetResourcesPath(request.Address), typeof(TAsset)) != null;
        }

        /// <inheritdoc/>
        public IAssetLoadHandle<TAsset> LoadAsync<TAsset>(IAssetRequest<TAsset> request)
            where TAsset : Object {
            if (!request.IsValid) {
                return new AssetLoadHandle<TAsset>(new ArgumentException("Request is not valid.", nameof(request)));
            }

            var resourcesPath = GetResourcesPath(request.Address);
            var resourceRequest = Resources.LoadAsync<TAsset>(resourcesPath);
            return new AssetLoadHandle<TAsset>(request.Address, resourceRequest);
        }

        /// <summary>
        /// Resources 用パスへ変換
        /// </summary>
        private string GetResourcesPath(string address) {
            var index = address.LastIndexOf("/Resources/", StringComparison.Ordinal);
            if (index >= 0) {
                address = address.Substring(index + "/Resources/".Length);
            }

            var extension = Path.GetExtension(address);
            if (!string.IsNullOrEmpty(extension)) {
                address = address.Substring(0, address.Length - extension.Length);
            }

            return address;
        }
    }
}
