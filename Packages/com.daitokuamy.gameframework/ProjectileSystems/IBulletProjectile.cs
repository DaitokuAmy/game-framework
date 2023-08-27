using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 弾制御用インターフェース
    /// </summary>
    public interface IBulletProjectile : IProjectile {
        // 座標
        Vector3 Position { get; }
        // 姿勢
        Quaternion Rotation { get; }
    }
}