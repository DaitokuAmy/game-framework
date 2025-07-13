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
        private readonly DisposableScope _unloadScope;
        private readonly PoolAssetStorage<ModelViewerPreviewActorMasterData> _previewActorMasterDataStorage;
        private readonly SimpleAssetStorage<ModelViewerEnvironmentActorMasterData> _environmentActorMasterDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerRepository() {
            _unloadScope = new DisposableScope();
            var assetManager = Services.Resolve<AssetManager>();
            _previewActorMasterDataStorage = new PoolAssetStorage<ModelViewerPreviewActorMasterData>(assetManager, 2);
            _environmentActorMasterDataStorage = new SimpleAssetStorage<ModelViewerEnvironmentActorMasterData>(assetManager);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _previewActorMasterDataStorage.Dispose();
            _unloadScope.Dispose();
        }

        /// <summary>
        /// ActorMasterの読み込み
        /// </summary>
        public async UniTask<IPreviewActorMaster> LoadActorMasterAsync(string assetKey, CancellationToken ct) {
            var setupData = await _previewActorMasterDataStorage
                .LoadAssetAsync(new ModelViewerActorMasterDataRequest(assetKey))
                .ToUniTask(cancellationToken:ct);

            return setupData;
        }

        /// <summary>
        /// EnvironmentMasterの読み込み
        /// </summary>
        public async UniTask<IEnvironmentActorMaster> LoadEnvironmentMasterAsync(string assetKey, CancellationToken ct) {
            var setupData = await _environmentActorMasterDataStorage
                .LoadAssetAsync(new ModelViewerEnvironmentMasterDataRequest(assetKey))
                .ToUniTask(cancellationToken:ct);

            return setupData;
        }
    }
}
