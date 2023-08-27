using System.Threading;
using Cysharp.Threading.Tasks;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// モデルビューア用のアプリケーション層サービス
    /// </summary>
    public class VfxViewerApplicationService {
        private VfxViewerModel _model;
        private VfxViewerRepository _repository;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VfxViewerApplicationService(VfxViewerModel model, VfxViewerRepository repository) {
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
        /// Vfxの変更
        /// </summary>
        public async UniTask<PreviewVfxSetupData> ChangePreviewVfxAsync(string setupDataId, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            // 設定ファイルを読み込み
            var result = await _repository.LoadActorSetupDataAsync(setupDataId, ct);
            
            // Modelに反映
            _model.PreviewVfxModel.Setup(setupDataId, result);
            
            // そのまま再生
            _model.PreviewVfxModel.Play();

            return result;
        }

        /// <summary>
        /// Vfxの再生
        /// </summary>
        public void PlayCurrentVfx() {
            _model.PreviewVfxModel.Play();
        }

        /// <summary>
        /// Vfxの停止
        /// </summary>
        public void StopCurrentVfx() {
            _model.PreviewVfxModel.Stop();
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
