using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.Core;
using ThirdPersonEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用のアセットリポジトリ
    /// </summary>
    public class ModelViewerAssetRepository : IDisposable, IServiceUser {
        private readonly DisposableScope _scope;
        
        private SimpleAssetStorage<PreviewActorData> _previewActorDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerAssetRepository() {
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _scope.Dispose();
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            var assetManager = resolver.Resolve<AssetManager>();
            _previewActorDataStorage = new SimpleAssetStorage<PreviewActorData>(assetManager).RegisterTo(_scope);
        }

        /// <summary>
        /// ActorMasterの読み込み
        /// </summary>
        public async UniTask<PreviewActorData> LoadPreviewActorDataAsync(string assetKey, CancellationToken ct) {
            var data = await _previewActorDataStorage
                .LoadAssetAsync(new PreviewActorDataRequest(assetKey))
                .ToUniTask(cancellationToken:ct);

            return data;
        }
    }
}
