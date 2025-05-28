using System;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 衝突検知用リスナー
    /// </summary>
    public interface IRaycastCollisionListener {
        /// <summary>
        /// 当たり判定の発生通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void OnHitRaycastCollision(RaycastHitResult result);
    }

    /// <summary>
    /// 汎用衝突検知用リスナー
    /// </summary>
    public class RaycastCollisionListener : IRaycastCollisionListener {
        // 当たり判定発生通知
        public event Action<RaycastHitResult> OnHitRaycastCollisionEvent;

        /// <summary>
        /// 当たり判定の発生通知
        /// </summary>
        void IRaycastCollisionListener.OnHitRaycastCollision(RaycastHitResult result) {
            OnHitRaycastCollisionEvent?.Invoke(result);
        }
    }
}