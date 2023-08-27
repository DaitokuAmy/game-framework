using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine.SceneManagement;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// プリロード用アセットストレージ
    /// </summary>
    public class PreloadSceneAssetStorage : SceneAssetStorage {
        /// <summary>
        /// 読み込み待ち情報
        /// </summary>
        public struct LoadHandle : IProcess<IReadOnlyList<SceneAssetHandle>> {
            // 読み込み中のAssetHandleリスト
            private SceneAssetHandle[] _assetHandles;
            
            // IEnumerator用
            object IEnumerator.Current => null;

            /// <summary>読み込み対象のAssetHandleリスト</summary>
            public IReadOnlyList<SceneAssetHandle> Result => _assetHandles;
            /// <summary>読み込み完了しているか</summary>
            public bool IsDone {
                get {
                    if (_assetHandles == null) {
                        return true;
                    }
                    
                    foreach (var handle in _assetHandles) {
                        if (!handle.IsDone) {
                            return false;
                        }
                    }

                    return true;
                }
            }
            /// <summary>エラー内容</summary>
            public Exception Exception {
                get {
                    if (_assetHandles == null) {
                        return null;
                    }
                    
                    foreach (var handle in _assetHandles) {
                        if (handle.Exception != null) {
                            return handle.Exception;
                        }
                    }

                    return null;
                }
            }
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public LoadHandle(SceneAssetHandle[] assetHandles) {
                _assetHandles = assetHandles;
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
        
        // キャッシュ情報
        private class CacheInfo {
            public SceneAssetHandle handle;
        }
        
        // キャッシュ
        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        public PreloadSceneAssetStorage(AssetManager assetManager) : base(assetManager) {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public override void Dispose() {
            UnloadAssets();
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        public LoadHandle LoadAssetsAsync(IEnumerable<SceneAssetRequest> requests) {
            var handles = new List<SceneAssetHandle>();
            
            foreach (var request in requests) {
                var address = request.Address;

                if (_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                    handles.Add(cacheInfo.handle);
                }
                else {
                    // ハンドルを取得してキャッシュ
                    var handle = LoadAssetAsyncInternal(request);
                    _cacheInfos[address] = new CacheInfo {
                        handle = handle
                    };
                    handles.Add(handle);
                }
            }
            
            return new LoadHandle(handles.ToArray());
        }
        
        /// <summary>
        /// 読み込み処理
        /// </summary>
        public LoadHandle LoadAssetAsync(SceneAssetRequest request) {
            var handles = new SceneAssetHandle[1];
            var address = request.Address;
            
            if (_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                handles[0] = cacheInfo.handle;
            }
            else {
                // ハンドルを取得してキャッシュ
                var handle = LoadAssetAsyncInternal(request);
                _cacheInfos[address] = new CacheInfo {
                    handle = handle
                };
                handles[0] = handle;
            }
            
            return new LoadHandle(handles);
        }

        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public Scene GetAsset(string address) {
            if (!_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                return new Scene();
            }

            return cacheInfo.handle.Scene;
        }


        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public Scene GetAsset(SceneAssetRequest request) {
            return GetAsset(request.Address);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void UnloadAssets() {
            foreach (var info in _cacheInfos.Values) {
                info.handle.Release();
            }
            _cacheInfos.Clear();
        }
    }
}