using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using Object = UnityEngine.Object;

namespace GameFramework.AssetSystems {
    /// <summary>
    /// プリロード用アセットストレージ
    /// </summary>
    public class PreloadAssetStorage<TAsset> : AssetStorage
        where TAsset : Object {
        // 読み込み待ち情報
        public struct LoadHandle : IProcess<IReadOnlyList<AssetHandle<TAsset>>> {
            // 読み込み中のAssetHandleリスト
            private AssetHandle<TAsset>[] _assetHandles;
            
            // IEnumerator用
            object IEnumerator.Current => null;

            /// <summary>読み込み結果</summary>
            public IReadOnlyList<AssetHandle<TAsset>> Result => _assetHandles;
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
            public LoadHandle(AssetHandle<TAsset>[] assetHandles) {
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
            public AssetHandle<TAsset> handle;
        }
        
        // キャッシュ
        private readonly Dictionary<string, CacheInfo> _cacheInfos = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManager">読み込みに使用するAssetManager</param>
        public PreloadAssetStorage(AssetManager assetManager) : base(assetManager) {
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
        public LoadHandle LoadAssetsAsync(IEnumerable<AssetRequest<TAsset>> requests) {
            var handles = new List<AssetHandle<TAsset>>();
            
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
        public LoadHandle LoadAssetAsync(AssetRequest<TAsset> request) {
            var handles = new AssetHandle<TAsset>[1];
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
        public TAsset GetAsset(string address) {
            if (!_cacheInfos.TryGetValue(address, out var cacheInfo)) {
                return null;
            }

            return cacheInfo.handle.Asset;
        }


        /// <summary>
        /// 読み込み済みアセットの取得
        /// </summary>
        public TAsset GetAsset(AssetRequest<TAsset> request) {
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