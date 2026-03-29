using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// 背景シーンアセット用のリポジトリ
    /// </summary>
    public class EnvironmentSceneRepository : System.IDisposable {
        private readonly SimpleSceneStorage _environmentSceneStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentSceneRepository() {
            _environmentSceneStorage = new SimpleSceneStorage(AssetUtility.CreateSceneLoaders());
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            _environmentSceneStorage.Dispose();
        }

        /// <summary>
        /// フィールドシーンの読み込み
        /// </summary>
        public UniTask<Scene> LoadFieldSceneAsync(string assetKey, CancellationToken ct) {
            var request = new FieldSceneRequest(assetKey, activateOnLoad: false);
            return LoadSceneAsyncInternal(request, ct);
        }

        /// <summary>
        /// フィールドシーンのアンロード
        /// </summary>
        public void UnloadFieldScene(string assetKey) {
            var request = new FieldSceneRequest(assetKey, activateOnLoad: false);
            UnloadSceneInternal(request);
        }

        /// <summary>
        /// シーンの読み込み
        /// </summary>
        private async UniTask<Scene> LoadSceneAsyncInternal(FieldSceneRequest request, CancellationToken ct) {
            var process = _environmentSceneStorage.LoadAsync(request);
            await process.ToUniTask(cancellationToken: ct);

            if (process.Exception != null) {
                Debug.LogException(process.Exception);
                return default;
            }

            await process.ActivateAsync().ToUniTask(cancellationToken: ct);

            return process.Scene;
        }

        /// <summary>
        /// シーンのアンロード
        /// </summary>
        private void UnloadSceneInternal(FieldSceneRequest request) {
            _environmentSceneStorage.Unload(request);
        }
    }
}
