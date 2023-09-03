namespace SampleGame.VfxViewer {
    /// <summary>
    /// VfxViewerSetupData読み込みリクエスト
    /// </summary>
    public class VfxViewerSetupGameRequest : GameAssetRequest<VfxViewerSetupData> {
        public override string Address { get; }
        
        public VfxViewerSetupGameRequest() {
            Address = GetPath("VfxViewer/dat_vfx_viewer_setup.asset");
        }
    }
    
    /// <summary>
    /// Preview用のVfxSetupData読み込みリクエスト
    /// </summary>
    public class PreviewVfxSetupGameRequest : GameAssetRequest<PreviewVfxSetupData> {
        public override string Address { get; }
        
        public PreviewVfxSetupGameRequest(string assetKey) {
            Address = GetPath($"VfxViewer/PreviewVfxSetupData/dat_preview_vfx_setup_{assetKey}.asset");
        }
    }
}