using GameFramework.ActorSystem;

namespace SampleGameEngine {
    /// <summary>
    /// 移動制御用インターフェース
    /// </summary>
    public interface IMovableActor : IMovable {
        /// <summary>地面の高さ</summary>
        float GroundHeight { get; }
        /// <summary>地上にいるか</summary>
        bool IsGrounded { get; }
    }
}
