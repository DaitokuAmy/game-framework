using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューア用のアプリケーション層サービス
    /// </summary>
    public class ModelViewerApplicationService {
        private ModelViewerModel _model;
        private ModelViewerRepository _repository;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerApplicationService(ModelViewerModel model, ModelViewerRepository repository) {
            _model = model;
            _repository = repository;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public async UniTask SetupAsync(CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            // 設定ファイル読み込み
            var result = await _repository.LoadSetupDataAsync(ct);
            
            // Modelに反映
            _model.Setup(result);
        }

        /// <summary>
        /// 表示モデルの変更
        /// </summary>
        public async UniTask<PreviewActorSetupData> ChangePreviewActorAsync(string setupDataId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            // 設定ファイルを読み込み
            var result = await _repository.LoadActorSetupDataAsync(setupDataId, ct);
            
            // Modelに反映
            _model.PreviewActorModel.Setup(setupDataId, result);

            return result;
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeEnvironment(string assetId) {
            // Modelに反映
            _model.EnvironmentModel.SetAssetId(assetId);
        }
    }
}
