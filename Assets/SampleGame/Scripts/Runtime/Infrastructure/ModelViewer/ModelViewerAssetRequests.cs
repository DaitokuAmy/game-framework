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
    public class ModelViewerActorSetupDataRequest : GameAssetRequest<ModelViewerActorSetupData> {
        public override string Address { get; }
        
        public ModelViewerActorSetupDataRequest(string assetKey) {
            Address = GetPath($"ModelViewer/ActorSetup/dat_model_viewer_actor_setup_{assetKey}.asset");
        }
    }
    
    /// <summary>
    /// ModelViewer用のEnvironmentSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerEnvironmentSetupDataRequest : GameAssetRequest<ModelViewerEnvironmentSetupData> {
        public override string Address { get; }
        
        public ModelViewerEnvironmentSetupDataRequest(string assetKey) {
            Address = GetPath($"ModelViewer/EnvironmentSetup/dat_model_viewer_environment_setup_{assetKey}.asset");
        }
    }
}