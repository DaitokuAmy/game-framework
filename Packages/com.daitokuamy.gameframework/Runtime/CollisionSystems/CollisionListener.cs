using System;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 衝突検知用リスナー
    /// </summary>
    public interface ICollisionListener {
        /// <summary>
        /// 当たり判定の発生通知
        /// </summary>
        /// <param name="result">当たり結果</param>
        void OnHitCollision(HitResult result);
    }

    /// <summary>
    /// 汎用衝突検知用リスナー
    /// </summary>
    public class CollisionListener : ICollisionListener {
        // 当たり判定発生通知
        public event Action<HitResult> OnHitCollisionEvent;

        /// <summary>
        /// 当たり判定の発生通知
        /// </summary>
        void ICollisionListener.OnHitCollision(HitResult result) {
            OnHitCollisionEvent?.Invoke(result);
        }
    }
}