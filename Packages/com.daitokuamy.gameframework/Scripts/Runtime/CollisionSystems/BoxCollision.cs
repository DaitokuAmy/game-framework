using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// OBBコリジョン
    /// </summary>
    public class BoxCollision : Collision {
        // 中心座標
        public override Vector3 Center { get; set; }

        // ハーフサイズ
        public Vector3 HalfExtents { get; set; }

        // 姿勢
        public Quaternion Orientation { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="halfExtents">ハーフサイズ</param>
        /// <param name="orientation">姿勢</param>
        public BoxCollision(Vector3 center, Vector3 halfExtents, Quaternion orientation) {
            Center = center;
            HalfExtents = halfExtents;
            Orientation = orientation;
        }

        /// <summary>
        /// 当たり判定実行
        /// </summary>
        /// <param name="layerMask">衝突対象のLayerMask</param>
        /// <param name="hitResults">判定格納用配列</param>
        /// <returns>衝突有効数</returns>
        protected override int HitCheck(int layerMask, Collider[] hitResults) {
            return Physics.OverlapBoxNonAlloc(Center, HalfExtents, hitResults, Orientation, layerMask);
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        protected override void DrawGizmosInternal() {
            var prevMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(Center, Orientation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, HalfExtents);
            Gizmos.matrix = prevMatrix;
        }
    }
}