using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// 移動を適用できる対象
    /// </summary>
    public interface IMovable {
        /// <summary>現在位置</summary>
        Vector3 Position { get; }
        /// <summary>現在回転</summary>
        Quaternion Rotation { get; }

        /// <summary>
        /// 移動量を適用する
        /// </summary>
        /// <param name="worldDelta">ワールド空間の移動量</param>
        /// <param name="warp">ワープ移動かどうか</param>
        void ApplyMove(Vector3 worldDelta, bool warp = false);

        /// <summary>
        /// 回転を適用する
        /// </summary>
        /// <param name="rotation">適用するワールド回転</param>
        void ApplyRotation(Quaternion rotation);
    }
}