using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用のリポジトリ
    /// </summary>
    public class ModelViewerRepository : IDisposable, IModelViewerRepository {
        private readonly AssetManager _assetManager;
        private readonly DisposableScope _unloadScope;
        private readonly PoolAssetStorage<ModelViewerActorMasterData> _actorSetupDataStorage;
        private readonly SimpleAssetStorage<ModelViewerEnvironmentMasterData> _environmentSetupDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerRepository(AssetManager assetManager) {
            _assetManager = assetManager;
            _unloadScope = new DisposableScope();
            _actorSetupDataStorage = new PoolAssetStorage<ModelViewerActorMasterData>(_assetManager, 2);
            _environmentSetupDataStorage = new SimpleAssetStorage<ModelViewerEnvironmentMasterData>(_assetManager);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _actorSetupDataStorage.Dispose();
            _unloadScope.Dispose();
        }

        /// <summary>
        /// ActorMasterの読み込み
        /// </summary>
        public async UniTask<IActorMaster> LoadActorMasterAsync(string assetKey, CancellationToken ct) {
            var setupData = await _actorSetupDataStorage
                .LoadAssetAsync(new ModelViewerActorMasterDataRequest(assetKey))
                .ToUniTask(cancellationToken:ct);

            return setupData;
        }

        /// <summary>
        /// EnvironmentMasterの読み込み
        /// </summary>
        public async UniTask<IEnvironmentMaster> LoadEnvironmentMasterAsync(string assetKey, CancellationToken ct) {
            var setupData = await _environmentSetupDataStorage
                .LoadAssetAsync(new ModelViewerEnvironmentMasterDataRequest(assetKey))
                .ToUniTask(cancellationToken:ct);

            return setupData;
        }
    }
}
