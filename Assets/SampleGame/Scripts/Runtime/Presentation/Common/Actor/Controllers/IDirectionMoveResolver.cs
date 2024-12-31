using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 向き指定移動解決用インターフェース
    /// </summary>
    public interface IDirectionMoveResolver : IMoveResolver {
        /// <summary>
        /// 向き指定移動(移動し続ける)
        /// </summary>
        /// <param name="direction">移動向き</param>
        /// <param name="speedMultiplier">速度係数</param>
        /// <param name="updateRotation">回転制御を行うか</param>
        void MoveToDirection(Vector3 direction, float speedMultiplier, bool updateRotation);
    }
}
