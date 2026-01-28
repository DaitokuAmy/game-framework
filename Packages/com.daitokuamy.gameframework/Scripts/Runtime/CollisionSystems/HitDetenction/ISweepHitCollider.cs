using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// スイープ（線分+半径）ヒットコライダーインターフェース
    /// </summary>
    public interface ISweepHitCollider {
        /// <summary>開始位置</summary>
        Vector3 Start { get; }
        /// <summary>終了位置</summary>
        Vector3 End { get; }
        /// <summary>厚み</summary>
        float Radius { get; }
    }
}