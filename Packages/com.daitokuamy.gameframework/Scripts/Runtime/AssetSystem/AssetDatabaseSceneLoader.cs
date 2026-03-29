using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

#if USE_ADDRESSABLES && UNITY_EDITOR
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GameFramework.AssetSystem {
    /// <summary>
    /// AssetDatabase用シーンローダー
    /// </summary>
    public sealed class AssetDatabaseSceneLoader : ISceneLoader {
        /// <summary>
        /// シーン読み込みハンドル
        /// </summary>
        private sealed class SceneLoadHandle : ISceneLoadHandle {
            private readonly string _path;
            private readonly bool _activateOnLoad;
            private AsyncOperation _asyncOperation;
            private AsyncOperator _activateOperator;
            private Scene _scene;
            private Exception _exception;
            private bool _isReleased;
            private bool _isUnloadStarted;

            /// <inheritdoc/>
            public bool IsDone => _isReleased || _asyncOperation == null || _asyncOperation.isDone;
            /// <inheritdoc/>
            public Scene Scene {
                get {
                    if (_asyncOperation == null || !_asyncOperation.isDone) {
                        return new Scene();
                    }

                    if (_scene.IsValid() || _exception != null) {
                        return _scene;
                    }

                    _scene = SceneManager.GetSceneByPath(_path);
                    if (!_scene.IsValid()) {
                        _exception = new FileNotFoundException($"Not found scene. [{_path}]");
                    }

                    return _scene;
                }
            }
            /// <inheritdoc/>
            public Exception Exception => _exception;
            /// <inheritdoc/>
            public bool IsValid => !_isReleased && _asyncOperation != null;
            /// <inheritdoc/>
            Scene IProcess<Scene>.Result => Scene;
            /// <inheritdoc/>
            object IEnumerator.Current => null;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public SceneLoadHandle(string path, bool activateOnLoad, AsyncOperation asyncOperation, Exception exception = null) {
                _path = path;
                _activateOnLoad = activateOnLoad;
                _asyncOperation = asyncOperation;
                _exception = exception;
                if (_asyncOperation == null) {
                    return;
                }

                _asyncOperation.allowSceneActivation = activateOnLoad;
                _asyncOperation.completed += OnCompleted;
            }

            /// <inheritdoc/>
            public AsyncOperationHandle ActivateAsync() {
                if (_activateOnLoad || _asyncOperation == null || _asyncOperation.isDone) {
                    return AsyncOperationHandle.CompletedHandle;
                }

                if (_activateOperator != null) {
                    return _activateOperator.GetHandle();
                }

                _activateOperator = new AsyncOperator();
                _asyncOperation.allowSceneActivation = true;
                return _activateOperator.GetHandle();
            }

            /// <inheritdoc/>
            public void Release() {
                if (_isReleased) {
                    return;
                }

                _isReleased = true;
                TryUnloadScene();
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
            private void OnCompleted(AsyncOperation asyncOperation) {
                _activateOperator?.Completed();
                if (_isReleased) {
                    TryUnloadScene();
                }
            }

            /// <summary>
            /// アンロードを試行
            /// </summary>
            private void TryUnloadScene() {
                if (_isUnloadStarted || (_asyncOperation != null && !_asyncOperation.isDone)) {
                    return;
                }

                var scene = Scene;
                if (!scene.IsValid() || !scene.isLoaded) {
                    return;
                }

                _isUnloadStarted = true;
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        /// <inheritdoc/>
        public bool CanLoad(ISceneRequest request) {
            if (!request.IsValid) {
                return false;
            }

#if UNITY_EDITOR
            var path = ResolveScenePath(request.Address);
            if (string.IsNullOrEmpty(path)) {
                return false;
            }

            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public ISceneLoadHandle LoadAsync(ISceneRequest request) {
            if (!request.IsValid) {
                return new SceneLoadHandle(request.Address, request.ActivateOnLoad, null, new ArgumentException("Request is not valid.", nameof(request)));
            }

#if UNITY_EDITOR
            var path = ResolveScenePath(request.Address);
            if (string.IsNullOrEmpty(path)) {
                return new SceneLoadHandle(request.Address, request.ActivateOnLoad, null, new FileNotFoundException($"Not found scene. [{request.Address}]"));
            }

            var asyncOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            return new SceneLoadHandle(path, request.ActivateOnLoad, asyncOperation, asyncOperation == null
                ? new FileNotFoundException($"Not found scene. [{request.Address}]")
                : null);
#else
            return new SceneLoadHandle(request.Address, request.ActivateOnLoad, null, new NotSupportedException("AssetDatabase is not supported outside editor."));
#endif
        }

        /// <summary>
        /// シーンパスを解決
        /// </summary>
        private static string ResolveScenePath(string address) {
#if USE_ADDRESSABLES && UNITY_EDITOR
            foreach (var locator in Addressables.ResourceLocators) {
                if (locator.Locate(address, typeof(SceneAsset), out var locations) && locations.Count > 0) {
                    return locations[0].InternalId;
                }
            }
#endif
            return address;
        }
    }
}
