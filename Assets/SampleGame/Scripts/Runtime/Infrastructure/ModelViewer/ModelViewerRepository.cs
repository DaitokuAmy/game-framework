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
        private readonly PoolAssetStorage<ModelViewerPreviewActorSetupData> _actorSetupDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerRepository(AssetManager assetManager) {
            _assetManager = assetManager;
            _unloadScope = new DisposableScope();
            _actorSetupDataStorage = new PoolAssetStorage<ModelViewerPreviewActorSetupData>(_assetManager, 2);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _actorSetupDataStorage.Dispose();
            _unloadScope.Dispose();
        }

        /// <summary>
        /// ActorDataの読み込み
        /// </summary>
        public async UniTask<IPreviewActorMaster> LoadActorMasterAsync(string setupDataId, CancellationToken ct) {
            var setupData = await _actorSetupDataStorage
                .LoadAssetAsync(new ModelViewerPreviewActorSetupDataRequest(setupDataId))
                .ToUniTask(cancellationToken:ct);

            return setupData;
        }
    }
}
