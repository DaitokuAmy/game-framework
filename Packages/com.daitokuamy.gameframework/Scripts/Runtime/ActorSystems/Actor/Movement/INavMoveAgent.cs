using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// MoveTo を NavMesh で実現するための移動エージェント抽象
    /// </summary>
    public interface INavMoveAgent {
        /// <summary>現在位置</summary>
        Vector3 Position { get; }
        /// <summary>残り距離</summary>
        float RemainingDistance { get; }
        /// <summary>現在速度</summary>
        Vector3 Velocity { get; }
        /// <summary>経路を保持しているかどうか</summary>
        bool HasPath { get; }

        /// <summary>
        /// 目的地を設定する
        /// </summary>
        /// <param name="destination">目的地</param>
        /// <returns>経路生成に成功した場合 true</returns>
        bool TrySetDestination(Vector3 destination);

        /// <summary>
        /// 経路をクリアする
        /// </summary>
        void ResetPath();
    }
}