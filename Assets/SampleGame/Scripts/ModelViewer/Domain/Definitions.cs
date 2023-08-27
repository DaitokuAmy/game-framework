using System;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// カメラの操作タイプ
    /// </summary>
    public enum CameraControlType {
        Default,
        SceneView,
    }

    /// <summary>
    /// 録画オプションマスク
    /// </summary>
    [Flags]
    public enum RecordingOptions {
        ActorRotation = 1 << 0,
        CameraRotation = 1 << 1,
        LightRotation = 1 << 2,
    }
}
