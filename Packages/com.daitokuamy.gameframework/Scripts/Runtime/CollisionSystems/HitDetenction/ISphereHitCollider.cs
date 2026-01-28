using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 球体ヒットコライダーインターフェース
    /// </summary>
    public interface ISphereHitCollider {
        /// <summary>スフィアの中心</summary>
        Vector3 Center { get; }
        /// <summary>スフィアの半径</summary>
        float Radius { get; }
    }
}