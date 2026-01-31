using UnityEngine;

namespace GameFramework.CollisionSystem {
    /// <summary>
    /// 受けコリジョン情報
    /// </summary>
    public interface IReceiveCollider {
        /// <summary>カプセルの開始位置</summary>
        Vector3 Start { get; }
        /// <summary>カプセルの終了位置</summary>
        Vector3 End { get; }
        /// <summary>カプセルの半径</summary>
        float Radius { get; }
    }
}