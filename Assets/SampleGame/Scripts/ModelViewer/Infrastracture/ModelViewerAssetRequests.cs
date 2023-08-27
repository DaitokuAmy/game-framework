namespace SampleGame.ModelViewer {
    /// <summary>
    /// ModelViewerSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerSetupDataRequest : DataAssetRequest<ModelViewerSetupData> {
        public override string Address { get; }
        
        public ModelViewerSetupDataRequest() {
            Address = GetPath($"ModelViewer/dat_model_viewer_setup.asset");
        }
    }
    
    /// <summary>
    /// Preview用のActorSetupData読み込みリクエスト
    /// </summary>
    public class PreviewActorSetupDataRequest : ActorAssetRequest<PreviewActorSetupData> {
        public override string Address { get; }
        
        public PreviewActorSetupDataRequest(string assetKey) {
            Address = GetPath($"Preview/dat_preview_actor_setup_{assetKey}.asset");
        }
    }
}