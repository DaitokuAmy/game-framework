using ThirdPersonEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewerConfigData用のAssetRequest
    /// </summary>
    public class ModelViewerConfigDataRequest : SystemAssetRequest<ModelViewerConfigData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerConfigDataRequest() : base("ModelViewer/Settings/dat_model_viewer_config.asset") {
        }
    }
    
    /// <summary>
    /// PreviewActorの初期化データ読み込みリクエスト
    /// </summary>
    public class PreviewActorDataRequest : ActorAssetRequest<PreviewActorData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorDataRequest(string assetKey) : base($"PreviewActor/dat_act_preview_{assetKey}.asset") {
        }
    }
}