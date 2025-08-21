using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// キャラアクター制御用ポート
    /// </summary>
    public interface ICharacterActorPort {
        /// <summary>位置</summary>
        Vector3 Position { get; }
        /// <summary>向き</summary>
        Quaternion Rotation { get; }
        /// <summary>注視向き</summary>
        Quaternion LookAtRotation { get; }

        /// <summary>
        /// 移動入力
        /// </summary>
        void InputMove(Vector2 input);

        /// <summary>
        /// 注視移動入力
        /// </summary>
        void InputLookAt(Vector2 input);

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