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
    /// ModelViewerMasterData用のAssetRequest
    /// </summary>
    public class ModelViewerMasterDataRequest : SystemAssetRequest<ModelViewerMasterData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerMasterDataRequest() : base("ModelViewer/Master/dat_model_viewer_master.asset") {
        }
    }
    
    /// <summary>
    /// ModelViewer用のActorSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerActorMasterDataRequest : SystemAssetRequest<ModelViewerPreviewActorMasterData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerActorMasterDataRequest(string assetKey) : base($"ModelViewer/Master/dat_model_viewer_actor_master_{assetKey}.asset") {
        }
    }
    
    /// <summary>
    /// ModelViewer用のEnvironmentSetupData読み込みリクエスト
    /// </summary>
    public class ModelViewerEnvironmentMasterDataRequest : SystemAssetRequest<ModelViewerEnvironmentActorMasterData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ModelViewerEnvironmentMasterDataRequest(string assetKey) : base($"ModelViewer/Master/dat_model_viewer_environment_master_{assetKey}.asset") {
        }
    }
}