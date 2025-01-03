namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewerSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerSetupDataRequest : GameAssetRequest<ModelViewerSetupData> {
        public override string Address { get; }
        
        public ModelViewerSetupDataRequest() {
            Address = GetPath($"ModelViewer/Settings/dat_model_viewer_setup.asset");
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