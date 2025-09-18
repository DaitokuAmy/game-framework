using Unity.Mathematics;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// カメラ操作用の設定のインタフェース
    /// </summary>
    public interface IPreviewCameraMaster {
        /// <summary>角度制御速度</summary>
        float AngleControlSpeed { get; }
        /// <summary>注視点オフセット制御速度</summary>
        float LookAtOffsetControlSpeed { get; }
        /// <summary>移動制御速度</summary>
        float DistanceControlSpeed { get; }
        /// <summary>スクロールによる移動速度制御</summary>
        float ScrollDistanceControlDistanceControlSpeed { get; }
        /// <summary>初期注視オフセット</summary>
        float3 StartLookAtOffset { get; }
        /// <summary>初期角度</summary>
        float3 StartAngles { get; }
        /// <summary>初期距離</summary>
        float StartDistance { get; }
    }
}