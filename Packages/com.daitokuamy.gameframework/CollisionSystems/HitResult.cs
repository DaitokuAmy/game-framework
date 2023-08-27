using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 衝突結果
    /// </summary>
    public struct HitResult {
        // 当たり発生位置
        public Vector3 center;
        // 衝突検知したCollider
        public Collider collider;
        // カスタムデータ
        public object customData;

        // 衝突位置（擬似計算）
        public Vector3 HitPoint => collider != null ? collider.ClosestPoint(center) : center;

        /// <summary>
        /// ヒット位置と法線向きを取得
        /// </summary>
        public (Vector3, Vector3) GetHitPointAndNormal() {
            if (collider == null) {
                return (center, Vector3.up);
            }

            var point = collider.ClosestPoint(center);
            var normal = (center - point).normalized;
            return (point, normal);
        }
    }
}