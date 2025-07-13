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

        /// <summary>
        /// 移動入力
        /// </summary>
        void InputMove(Vector2 input);

        /// <summary>
        /// ジャンプ入力
        /// </summary>
        void InputJump();
    }
}