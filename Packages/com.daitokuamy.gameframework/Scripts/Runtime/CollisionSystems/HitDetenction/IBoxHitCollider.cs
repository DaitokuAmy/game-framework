using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// ボックス（OBB）ヒットコライダーインターフェース
    /// </summary>
    public interface IBoxHitCollider {
        /// <summary>中心座標</summary>
        Vector3 Center { get; }
        /// <summary>回転</summary>
        Quaternion Rotation { get; }
        /// <summary>サイズ（半分）</summary>
        Vector3 HalfExtents { get; }
    }
}