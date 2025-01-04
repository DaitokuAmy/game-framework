using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// フィールドシーンアセット用のリポジトリ
    /// </summary>
    public class FieldSceneRepository : IDisposable {
        private readonly SimpleSceneAssetStorage _fieldStorage;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldSceneRepository(AssetManager assetManager) {
            _fieldStorage = new SimpleSceneAssetStorage(assetManager);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _fieldStorage.Dispose();
        }

        /// <summary>
        /// フィールドシーンの読み込み
        /// </summary>
        public async UniTask<Scene> LoadFieldSceneAsync(string assetKey, CancellationToken ct) {
            var request = new FieldSceneAssetRequest(assetKey);
            var handle = _fieldStorage.LoadAssetAsync(request);
            await handle.ToUniTask(cancellationToken:ct);

            if (handle.Exception != null) {
                Debug.LogException(handle.Exception);
                return default;
            }
            
            await handle.ActivateAsync().ToUniTask(cancellationToken: ct);
            return handle.Scene;
        }

        /// <summary>
        /// フィールドシーンのアンロード
        /// </summary>
        public void UnloadFieldScene(string assetKey) {
            var request = new FieldSceneAssetRequest(assetKey);
            var scene = _fieldStorage.GetAsset(request);
            SceneManager.UnloadSceneAsync(scene);
            _fieldStorage.UnloadAsset(request.Address);
        }
    }
}
