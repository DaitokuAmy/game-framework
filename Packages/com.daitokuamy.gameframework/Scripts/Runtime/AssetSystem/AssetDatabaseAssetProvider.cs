using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GameFramework.AssetSystem {
    /// <summary>
    /// AssetDatabaseを使ったアセット提供用クラス
    /// </summary>
    public sealed class AssetDatabaseAssetProvider : IAssetProvider {
        /// <summary>
        /// アセット情報
        /// </summary>
        private class AssetInfo<T> : IAssetInfo<T>
            where T : Object {
            private T _asset;

            /// <inheritdoc/>
            bool IAssetInfo<T>.IsDone => true;
            /// <inheritdoc/>
            T IAssetInfo<T>.Asset => _asset;
            /// <inheritdoc/>
            Exception IAssetInfo<T>.Exception => null;

            public AssetInfo(T asset) {
                _asset = asset;
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
            private string _path;
            private AsyncOperation _asyncOperation;
            private Scene _scene;
            private Exception _exception;

            /// <inheritdoc/>
            bool ISceneAssetInfo.IsDone => _asyncOperation == null || _asyncOperation.isDone;
            /// <inheritdoc/>
            Scene ISceneAssetInfo.Scene {
                get {
                    if (_asyncOperation == null || !_asyncOperation.isDone) {
                        return new Scene();
                    }

                    if (_scene.IsValid() || _exception != null) {
                        return _scene;
                    }

                    _scene = SceneManager.GetSceneByPath(_path);
                    if (!_scene.IsValid()) {
                        _exception = new Exception($"Not found scene. [{_path}]");
                    }

                    return _scene;
                }
            }
            /// <inheritdoc/>
            Exception ISceneAssetInfo.Exception => _exception;

            public SceneAssetInfo(string path, AsyncOperation asyncOperation) {
                _path = path;
                _asyncOperation = asyncOperation;
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
        AssetHandle<T> IAssetProvider.LoadAsync<T>(string address) {
#if UNITY_EDITOR
            // Address > Path変換
            var path = GetAssetPath<T>(address);

            // 読み込み処理
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            var info = new AssetInfo<T>(asset);
            return new AssetHandle<T>(info);
#else
            return AssetHandle<T>.Empty;
#endif
        }

        /// <inheritdoc/>
        bool IAssetProvider.Contains<T>(string address) {
#if UNITY_EDITOR
            // GUIDが存在しなければない扱い
            var guid = AssetDatabase.AssetPathToGUID(address);
            return !string.IsNullOrEmpty(guid);
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        SceneAssetHandle IAssetProvider.LoadSceneAsync(string address, LoadSceneMode mode) {
#if UNITY_EDITOR
            var asyncOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(address, new LoadSceneParameters(mode));
            var info = new SceneAssetInfo(address, asyncOperation);
            return new SceneAssetHandle(info);
#else
            return SceneAssetHandle.Empty;
#endif
        }

        /// <inheritdoc/>
        bool IAssetProvider.ContainsScene(string address) {
#if UNITY_EDITOR
            // GUIDが存在しなければない扱い
            var guid = AssetDatabase.AssetPathToGUID(address);
            return !string.IsNullOrEmpty(guid);
#else
            return false;
#endif
        }

        /// <summary>
        /// AddressをPathに変換
        /// </summary>
        private string GetAssetPath<T>(string address) {
#if USE_ADDRESSABLES && UNITY_EDITOR
            foreach (var locator in Addressables.ResourceLocators) {
                if (locator.Locate(address, typeof(T), out var list)) {
                    return list[0].InternalId;
                }
            }
#endif
            return address;
        }
    }
}
