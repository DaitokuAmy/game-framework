using Unity.Mathematics;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// キャラアクター制御用ポート
    /// </summary>
    public interface ICharacterActorPort {
        /// <summary>位置</summary>
        float3 Position { get; }
        /// <summary>向き</summary>
        quaternion Rotation { get; }
        /// <summary>注視向き</summary>
        quaternion LookAtRotation { get; }

        /// <summary>
        /// 移動入力
        /// </summary>
        void InputMove(float2 input);

        /// <summary>
        /// 注視移動入力
        /// </summary>
        void InputLookAt(float2 input);

        /// <summary>
        /// スプリント入力
        /// </summary>
        void InputSprint(bool sprint);

        /// <summary>
        /// ジャンプ入力
        /// </summary>
        void InputJump();
    }
}