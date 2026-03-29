using System;
using System.Collections;
using System.IO;
using Object = UnityEngine.Object;

#if USE_ADDRESSABLES && UNITY_EDITOR
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.AssetSystem {
    /// <summary>
    /// AssetDatabase用アセットローダー
    /// </summary>
    public sealed class AssetDatabaseAssetLoader : IAssetLoader {
        /// <summary>
        /// アセット読み込みハンドル
        /// </summary>
        private sealed class AssetLoadHandle<TAsset> : IAssetLoadHandle<TAsset>
            where TAsset : Object {
            private readonly TAsset _asset;
            private readonly Exception _exception;
            private bool _isReleased;

            /// <inheritdoc/>
            public bool IsDone => true;
            /// <inheritdoc/>
            public TAsset Asset => _isReleased ? null : _asset;
            /// <inheritdoc/>
            public Exception Exception => _exception;
            /// <inheritdoc/>
            public bool IsValid => !_isReleased && _asset != null;
            /// <inheritdoc/>
            TAsset IProcess<TAsset>.Result => Asset;
            /// <inheritdoc/>
            object IEnumerator.Current => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public AssetLoadHandle(TAsset asset, Exception exception = null) {
                _asset = asset;
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
                return false;
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

#if UNITY_EDITOR
            var path = ResolveAssetPath<TAsset>(request.Address);
            if (string.IsNullOrEmpty(path)) {
                return false;
            }

            return AssetDatabase.LoadAssetAtPath<TAsset>(path) != null;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public IAssetLoadHandle<TAsset> LoadAsync<TAsset>(IAssetRequest<TAsset> request)
            where TAsset : Object {
            if (!request.IsValid) {
                return new AssetLoadHandle<TAsset>(null, new ArgumentException("Request is not valid.", nameof(request)));
            }

#if UNITY_EDITOR
            var path = ResolveAssetPath<TAsset>(request.Address);
            if (string.IsNullOrEmpty(path)) {
                return new AssetLoadHandle<TAsset>(null, new FileNotFoundException($"Not found asset. [{request.Address}]"));
            }

            var asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
            return new AssetLoadHandle<TAsset>(asset, asset == null
                ? new FileNotFoundException($"Not found asset. [{request.Address}]")
                : null);
#else
            return new AssetLoadHandle<TAsset>(null, new NotSupportedException("AssetDatabase is not supported outside editor."));
#endif
        }

        /// <summary>
        /// アセットパスを解決
        /// </summary>
        private string ResolveAssetPath<TAsset>(string address)
            where TAsset : Object {
#if USE_ADDRESSABLES && UNITY_EDITOR
            foreach (var locator in Addressables.ResourceLocators) {
                if (locator.Locate(address, typeof(TAsset), out var locations) && locations.Count > 0) {
                    return locations[0].InternalId;
                }
            }
#endif
            return address;
        }
    }
}
