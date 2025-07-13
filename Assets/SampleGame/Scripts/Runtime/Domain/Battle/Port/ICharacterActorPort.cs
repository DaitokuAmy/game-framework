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
    }
}