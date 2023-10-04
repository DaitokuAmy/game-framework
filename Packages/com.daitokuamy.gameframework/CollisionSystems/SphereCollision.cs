using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 球体コリジョン
    /// </summary>
    public class SphereCollision : Collision {
        // 中心座標
        public override Vector3 Center { get; set; }

        // 半径
        public float Radius { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        public SphereCollision(Vector3 center, float radius) {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// 当たり判定実行
        /// </summary>
        /// <param name="layerMask">衝突対象のLayerMask</param>
        /// <param name="hitResults">判定格納用配列</param>
        /// <returns>衝突有効数</returns>
        protected override int HitCheck(int layerMask, Collider[] hitResults) {
            if (Physics.CheckSphere(Center, Radius, layerMask)) {
                
            }
            
            return Physics.OverlapSphereNonAlloc(Center, Radius, hitResults, layerMask);
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        protected override void DrawGizmosInternal() {
            var prevMatrix = Gizmos.matrix;
            var splitCount = 3;

            for (var y = 0; y < splitCount; y++) {
                var angleY = 90.0f * y / splitCount;
                Gizmos.matrix = Matrix4x4.TRS(Center, Quaternion.Euler(0.0f, angleY, 0.0f), Vector3.one);
                Gizmos.DrawWireSphere(Vector3.zero, Radius);
            }

            Gizmos.matrix = prevMatrix;
        }
    }
}