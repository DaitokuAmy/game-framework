using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// レイキャスト衝突結果
    /// </summary>
    public struct RaycastHitResult {
        /// <summary>衝突検知したCollider</summary>
        public RaycastHit RaycastHit;
        /// <summary>カスタムデータ</summary>
        public object CustomData;
        /// <summary>ヒットカウント</summary>
        public int HitCount;
    }
}