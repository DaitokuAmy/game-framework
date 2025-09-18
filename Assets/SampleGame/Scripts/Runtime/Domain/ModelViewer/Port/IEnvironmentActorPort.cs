using Unity.Mathematics;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 背景制御クラス
    /// </summary>
    public interface IEnvironmentActorPort {
        /// <summary>ルート位置</summary>
        float3 RootPosition { get; }
        /// <summary>ルート向き</summary>
        quaternion RootRotation { get; }
    }
}