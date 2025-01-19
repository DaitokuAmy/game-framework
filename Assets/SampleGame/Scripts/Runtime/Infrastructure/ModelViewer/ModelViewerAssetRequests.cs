namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewerConfigData用のAssetRequest
    /// </summary>
    public class ModelViewerConfigDataRequest : GameAssetRequest<ModelViewerConfigData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerConfigDataRequest() : base("ModelViewer/Settings/dat_model_viewer_config.asset") {
        }
    }
    
    /// <summary>
    /// ModelViewer用のActorSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerActorSetupDataRequest : GameAssetRequest<ModelViewerActorSetupData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerActorSetupDataRequest(string assetKey) : base($"ModelViewer/ActorSetup/dat_model_viewer_actor_setup_{assetKey}.asset") {
        }
    }
    
    /// <summary>
    /// ModelViewer用のEnvironmentSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerEnvironmentSetupDataRequest : GameAssetRequest<ModelViewerEnvironmentSetupData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerEnvironmentSetupDataRequest(string assetKey) : base($"ModelViewer/EnvironmentSetup/dat_model_viewer_environment_setup_{assetKey}.asset") {
        }
    }
}