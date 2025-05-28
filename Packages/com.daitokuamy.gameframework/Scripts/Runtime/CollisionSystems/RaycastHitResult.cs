using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// レイキャスト衝突結果
    /// </summary>
    public struct RaycastHitResult {
        // 衝突検知したCollider
        public RaycastHit raycastHit;
        // カスタムデータ
        public object customData;
        // ヒットカウント
        public int hitCount;
    }
}