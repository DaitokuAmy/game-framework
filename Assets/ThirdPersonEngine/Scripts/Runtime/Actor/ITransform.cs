using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// Transformを隠蔽するためのインターフェース
    /// </summary>
    public interface ITransform {
        /// <summary>座標</summary>
        Vector3 Position { get; }
        /// <summary>向き</summary>
        Quaternion Rotation { get; }
    }
}