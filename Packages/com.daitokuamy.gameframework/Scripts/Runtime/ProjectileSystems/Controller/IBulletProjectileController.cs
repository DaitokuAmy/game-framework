using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 弾制御用インターフェース
    /// </summary>
    public interface IBulletProjectileController : IProjectileController {
        /// <summary>座標</summary>
        Vector3 Position { get; }
        /// <summary>姿勢</summary>
        Quaternion Rotation { get; }
    }
}