using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 座標指定移動解決用インターフェース
    /// </summary>
    public interface IPointMoveResolver : IMoveResolver {
        /// <summary>
        /// 座標指定移動
        /// </summary>
        /// <param name="point">移動座標</param>
        /// <param name="updateRotation">回転制御を行うか</param>
        void MoveToPoint(Vector3 point, bool updateRotation);
    }
}
