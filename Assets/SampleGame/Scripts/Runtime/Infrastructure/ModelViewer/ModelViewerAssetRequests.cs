namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewerConfigData用のAssetRequest
    /// </summary>
    public class ModelViewerConfigDataRequest : GameAssetRequest<ModelViewerConfigData> {
        // 読み込みAddress
        public override string Address { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerConfigDataRequest() {
            Address = GetPath("ModelViewer/Settings/dat_model_viewer_config.asset");
        }
    }
    
    /// <summary>
    /// ModelViewer用のActorSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerPreviewActorSetupDataRequest : GameAssetRequest<ModelViewerPreviewActorSetupData> {
        public override string Address { get; }
        
        public ModelViewerPreviewActorSetupDataRequest(string assetKey) {
            Address = GetPath($"ModelViewer/PreviewActorSetup/dat_model_viewer_preview_actor_setup_{assetKey}.asset");
        }
    }
}