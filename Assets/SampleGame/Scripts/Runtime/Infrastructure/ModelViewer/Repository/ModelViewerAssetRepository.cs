using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystem;
using SampleGameEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用のアセットリポジトリ
    /// </summary>
    public class ModelViewerAssetRepository : System.IDisposable {
        private readonly SimpleAssetStorage _previewActorDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerAssetRepository() {
            _previewActorDataStorage = new SimpleAssetStorage(AssetUtility.CreateAssetLoaders());
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            _previewActorDataStorage.Dispose();
        }

        /// <summary>
        /// ActorMasterの読み込み
        /// </summary>
        public async UniTask<PreviewActorData> LoadPreviewActorDataAsync(string assetKey, CancellationToken ct) {
            var data = await _previewActorDataStorage
                .LoadAsync<PreviewActorData, PreviewActorDataRequest>(new PreviewActorDataRequest(assetKey))
                .ToUniTask<PreviewActorData>(cancellationToken: ct);

            return data;
        }
    }
}
