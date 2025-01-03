namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 操作用の設定のインタフェース
    /// </summary>
    public interface IPreviewCameraControllerContext {
        float MouseLeftDeltaSpeed { get; }
        float MouseMiddleDeltaSpeed { get; }
        float MouseRightDeltaSpeed { get; }
        float MouseScrollSpeed { get; }
    }
}