namespace SampleGame.VfxViewer {
    /// <summary>
    /// VfxViewerSetupData読み込みリクエスト
    /// </summary>
    public class VfxViewerSetupDataRequest : DataAssetRequest<VfxViewerSetupData> {
        public override string Address { get; }
        
        public VfxViewerSetupDataRequest() {
            Address = GetPath("VfxViewer/dat_vfx_viewer_setup.asset");
        }
    }
    
    /// <summary>
    /// Preview用のVfxSetupData読み込みリクエスト
    /// </summary>
    public class PreviewVfxSetupDataRequest : DataAssetRequest<PreviewVfxSetupData> {
        public override string Address { get; }
        
        public PreviewVfxSetupDataRequest(string assetKey) {
            Address = GetPath($"VfxViewer/PreviewVfxSetupData/dat_preview_vfx_setup_{assetKey}.asset");
        }
    }
}