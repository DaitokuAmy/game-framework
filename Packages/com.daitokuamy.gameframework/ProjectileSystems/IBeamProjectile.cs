using UnityEngine;

namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// ビーム制御用インターフェース
    /// </summary>
    public interface IBeamProjectile : IProjectile {
        /// <summary>先端座標</summary>
        Vector3 HeadPosition { get; }
        /// <summary>末端座標</summary>
        Vector3 TailPosition { get; }
        /// <summary>姿勢</summary>
        Quaternion Rotation { get; }
        /// <summary>飛翔物の長さ</summary>
        float Distance { get; }
        /// <summary>太さの割合(0～1)</summary>
        float Thickness { get; }
        /// <summary>実体化しているか</summary>
        bool IsSolid { get; }
        /// <summary>衝突中か</summary>
        bool IsHitting { get; }
    }
}