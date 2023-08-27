using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 向き指定移動解決用インターフェース
    /// </summary>
    public interface IDirectionMoveResolver : IMoveResolver {
        /// <summary>
        /// 座標指定移動
        /// </summary>
        /// <param name="direction">移動向き</param>
        /// <param name="updateRotation">回転制御を行うか</param>
        void MoveDirection(Vector3 direction, bool updateRotation);
    }
}
