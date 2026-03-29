using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// 単純な SceneStorage
    /// </summary>
    public sealed class SimpleSceneStorage : SceneStorage {
        /// <summary>
        /// キャッシュ情報
        /// </summary>
        private sealed class CacheInfo {
            /// <summary>シーン読み込みハンドル</summary>
            public readonly ISceneLoadHandle Handle;
            /// <summary>シーン読み込み結果</summary>
            public readonly SceneProcess Process;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CacheInfo(ISceneLoadHandle handle) {
                Handle = handle;
                Process = new SceneProcess(handle);
            }

            /// <summary>有効か</summary>
            public bool IsValid => Handle.IsValid;

            /// <summary>
            /// 解放
            /// </summary>
            public void Release() {
                Handle.Release();
            }
        }

        /// <summary>
        /// Handle を ISceneProcess として扱うラッパー
        /// </summary>
        private sealed class SceneProcess : ISceneProcess {
            private readonly ISceneLoadHandle _handle;

            /// <inheritdoc/>
            public bool IsDone => _handle.IsDone;
            /// <inheritdoc/>
            public Exception Exception => _handle.Exception;
            /// <inheritdoc/>
            public Scene Scene => _handle.Scene;
            /// <inheritdoc/>
            public bool IsValid => _handle.IsValid;
            /// <inheritdoc/>
            public Scene Result => _handle.Scene;
            /// <inheritdoc/>
            public object Current => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public SceneProcess(ISceneLoadHandle handle) {
                _handle = handle;
            }

            /// <inheritdoc/>
            public AsyncOperationHandle ActivateAsync() {
                return _handle.ActivateAsync();
            }

            /// <inheritdoc/>
            public bool MoveNext() {
                return !_handle.IsDone;
            }

            /// <inheritdoc/>
            public void Reset() {
                throw new NotImplementedException();
            }
        }

        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SimpleSceneStorage(params ISceneLoader[] loaders) : base(loaders) {
        }

        /// <inheritdoc/>
        public override ISceneProcess LoadAsync<TRequest>(TRequest request) {
            if (_cacheInfos.TryGetValue(request.Address, out var cacheInfo)) {
                if (request.ActivateOnLoad) {
                    cacheInfo.Process.ActivateAsync();
                }

                return cacheInfo.Process;
            }

            var handle = LoadSceneAsyncInternal(request);
            var createdCacheInfo = new CacheInfo(handle);
            if (createdCacheInfo.IsValid) {
                _cacheInfos[request.Address] = createdCacheInfo;
            }

            return createdCacheInfo.Process;
        }

        /// <inheritdoc/>
        public override void Unload<TRequest>(TRequest request) {
            if (!_cacheInfos.TryGetValue(request.Address, out var cacheInfo)) {
                return;
            }

            cacheInfo.Release();
            _cacheInfos.Remove(request.Address);
        }

        /// <inheritdoc/>
        public override void Clear() {
            foreach (var cacheInfo in _cacheInfos.Values) {
                cacheInfo.Release();
            }

            _cacheInfos.Clear();
        }
    }
}
