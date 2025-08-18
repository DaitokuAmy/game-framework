using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Domain.ModelViewer;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用のリポジトリ
    /// </summary>
    public class ModelViewerRepository : IDisposable, IModelViewerRepository {
        private readonly DisposableScope _unloadScope;
        private readonly SimpleAssetStorage<ModelViewerMasterData> _masterDataStorage;
        private readonly PoolAssetStorage<ModelViewerPreviewActorMasterData> _previewActorMasterDataStorage;
        private readonly PoolAssetStorage<ModelViewerEnvironmentActorMasterData> _environmentActorMasterDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerRepository() {
            _unloadScope = new DisposableScope();
            var assetManager = Services.Resolve<AssetManager>();
            _masterDataStorage = new SimpleAssetStorage<ModelViewerMasterData>(assetManager).RegisterTo(_unloadScope);
            _previewActorMasterDataStorage = new PoolAssetStorage<ModelViewerPreviewActorMasterData>(assetManager, 2).RegisterTo(_unloadScope);
            _environmentActorMasterDataStorage = new PoolAssetStorage<ModelViewerEnvironmentActorMasterData>(assetManager, 2).RegisterTo(_unloadScope);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _unloadScope.Dispose();
        }

        /// <summary>
        /// Masterの読み込み
        /// </summary>
        public async UniTask<IModelViewerMaster> LoadMasterAsync(CancellationToken ct) {
            var data = await _masterDataStorage
                .LoadAssetAsync(new ModelViewerMasterDataRequest())
                .ToUniTask(cancellationToken:ct);

            return data;
        }

        /// <summary>
        /// ActorMasterの読み込み
        /// </summary>
        public async UniTask<IPreviewActorMaster> LoadActorMasterAsync(string assetKey, CancellationToken ct) {
            var data = await _previewActorMasterDataStorage
                .LoadAssetAsync(new ModelViewerActorMasterDataRequest(assetKey))
                .ToUniTask(cancellationToken:ct);

            return data;
        }

        /// <summary>
        /// EnvironmentMasterの読み込み
        /// </summary>
        public async UniTask<IEnvironmentActorMaster> LoadEnvironmentMasterAsync(string assetKey, CancellationToken ct) {
            var data = await _environmentActorMasterDataStorage
                .LoadAssetAsync(new ModelViewerEnvironmentMasterDataRequest(assetKey))
                .ToUniTask(cancellationToken:ct);

            return data;
        }
    }
}
