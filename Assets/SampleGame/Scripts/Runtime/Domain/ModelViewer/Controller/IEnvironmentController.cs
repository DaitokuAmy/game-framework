namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 背景制御クラス
    /// </summary>
    public interface IEnvironmentController {
        /// <summary>ディレクショナルライトのY角度</summary>
        float LightAngleY { get; }
        
        /// <summary>
        /// ディレクショナルライトのY角度を設定
        /// </summary>
        void SetLightAngleY(float angleY);
    }
}