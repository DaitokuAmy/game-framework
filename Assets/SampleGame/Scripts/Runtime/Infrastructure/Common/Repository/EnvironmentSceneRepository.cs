using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// 背景シーンアセット用のリポジトリ
    /// </summary>
    public class EnvironmentSceneRepository : IDisposable {
        private readonly SimpleSceneAssetStorage _environmentSceneAssetStorage;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentSceneRepository() {
            var assetManager = Services.Resolve<AssetManager>();
            _environmentSceneAssetStorage = new SimpleSceneAssetStorage(assetManager);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _environmentSceneAssetStorage.Dispose();
        }

        /// <summary>
        /// フィールドシーンの読み込み
        /// </summary>
        public UniTask<Scene> LoadFieldSceneAsync(string assetKey, CancellationToken ct) {
            var request = new FieldSceneAssetRequest(assetKey);
            return LoadSceneAsyncInternal(request, ct);
        }

        /// <summary>
        /// フィールドシーンのアンロード
        /// </summary>
        public void UnloadFieldScene(string assetKey) {
            var request = new FieldSceneAssetRequest(assetKey);
            UnloadSceneInternal(request);
        }

        /// <summary>
        /// シーンの読み込み
        /// </summary>
        private async UniTask<Scene> LoadSceneAsyncInternal(EnvironmentSceneAssetRequest request, CancellationToken ct) {
            var handle = _environmentSceneAssetStorage.LoadAssetAsync(request);
            await handle.ToUniTask(cancellationToken:ct);

            if (handle.Exception != null) {
                Debug.LogException(handle.Exception);
                return default;
            }
            
            await handle.ActivateAsync().ToUniTask(cancellationToken: ct);
            return handle.Scene;
        }

        /// <summary>
        /// シーンのアンロード
        /// </summary>
        private void UnloadSceneInternal(EnvironmentSceneAssetRequest request) {
            var scene = _environmentSceneAssetStorage.GetAsset(request);
            SceneManager.UnloadSceneAsync(scene);
            _environmentSceneAssetStorage.UnloadAsset(request.Address);
        }
    }
}
