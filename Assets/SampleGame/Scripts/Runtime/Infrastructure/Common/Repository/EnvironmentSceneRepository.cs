using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// 背景シーンアセット用のリポジトリ
    /// </summary>
    public class EnvironmentSceneRepository : IDisposable, IServiceUser {
        private readonly DisposableScope _scope;

        private IServiceResolver _serviceResolver;
        private SimpleSceneAssetStorage _environmentSceneAssetStorage;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentSceneRepository() {
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _scope.Dispose();
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            _serviceResolver = resolver;
            var assetManager = resolver.Resolve<AssetManager>();
            _environmentSceneAssetStorage = new SimpleSceneAssetStorage(assetManager).RegisterTo(_scope);
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

            var serviceUsers = handle.Scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<IServiceUser>())
                .ToArray();
            foreach (var user in serviceUsers) {
                _serviceResolver.Import(user);
            }
            
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
