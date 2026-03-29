#if USE_ADDRESSABLES

using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystem {
    /// <summary>
    /// Addressables用シーンローダー
    /// </summary>
    public sealed class AddressablesSceneLoader : ISceneLoader {
        /// <summary>
        /// シーン読み込みハンドル
        /// </summary>
        private sealed class SceneLoadHandle : ISceneLoadHandle {
            private readonly bool _activateOnLoad;
            private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<SceneInstance> _handle;
            private AsyncOperator _activateOperator;
            private bool _isReleased;
            private bool _isUnloadStarted;

            /// <inheritdoc/>
            public bool IsDone => _isReleased || !_handle.IsValid() || _handle.IsDone;
            /// <inheritdoc/>
            public Scene Scene => _handle.IsValid() && _handle.Status == AsyncOperationStatus.Succeeded
                ? _handle.Result.Scene
                : new Scene();
            /// <inheritdoc/>
            public Exception Exception => _handle.IsValid() ? _handle.OperationException : null;
            /// <inheritdoc/>
            public bool IsValid => !_isReleased && _handle.IsValid();
            /// <inheritdoc/>
            Scene IProcess<Scene>.Result => Scene;
            /// <inheritdoc/>
            object IEnumerator.Current => null;

            /// <summary>
            /// コンストラクター
            /// </summary>
            public SceneLoadHandle(
                bool activateOnLoad,
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<SceneInstance> handle) {
                _activateOnLoad = activateOnLoad;
                _handle = handle;
                _handle.Completed += OnCompleted;
            }

            /// <inheritdoc/>
            public AsyncOperationHandle ActivateAsync() {
                if (_activateOnLoad) {
                    return AsyncOperationHandle.CompletedHandle;
                }

                if (_activateOperator != null) {
                    return _activateOperator.GetHandle();
                }

                _activateOperator = new AsyncOperator();
                if (_handle.IsDone) {
                    BeginActivate();
                }
                else {
                    _handle.Completed += _ => BeginActivate();
                }

                return _activateOperator.GetHandle();
            }

            /// <inheritdoc/>
            public void Release() {
                if (_isReleased) {
                    return;
                }

                _isReleased = true;
                TryReleaseOrUnload();
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

            /// <summary>
            /// 読み込み完了時の処理
            /// </summary>
            private void OnCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<SceneInstance> handle) {
                if (_isReleased) {
                    TryReleaseOrUnload();
                }
            }

            /// <summary>
            /// アクティブ化を開始
            /// </summary>
            private void BeginActivate() {
                if (_activateOperator == null || _activateOperator.IsDone) {
                    return;
                }

                if (!_handle.IsValid()) {
                    _activateOperator.Aborted(new InvalidOperationException("Scene handle is not valid."));
                    return;
                }

                if (_handle.Status != AsyncOperationStatus.Succeeded) {
                    _activateOperator.Aborted(_handle.OperationException ?? new InvalidOperationException("Scene load failed."));
                    return;
                }

                var asyncOperation = _handle.Result.ActivateAsync();
                if (asyncOperation == null) {
                    _activateOperator.Completed();
                    return;
                }

                asyncOperation.completed += _ => _activateOperator.Completed();
            }

            /// <summary>
            /// 解放またはアンロードを実行
            /// </summary>
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
        public bool CanLoad(ISceneRequest request) {
            if (!request.IsValid) {
                return false;
            }

            foreach (var locator in Addressables.ResourceLocators) {
                if (locator.Locate(request.Address, typeof(SceneInstance), out _)) {
                    return true;
                }

                if (locator.Locate(request.Address, typeof(object), out _)) {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public ISceneLoadHandle LoadAsync(ISceneRequest request) {
            var operationHandle = Addressables.LoadSceneAsync(request.Address, LoadSceneMode.Additive, request.ActivateOnLoad);
            return new SceneLoadHandle(request.ActivateOnLoad, operationHandle);
        }
    }
}

#endif
