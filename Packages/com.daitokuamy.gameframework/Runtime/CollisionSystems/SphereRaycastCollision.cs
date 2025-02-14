using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// SphereRaycastコリジョン
    /// </summary>
    public class SphereRaycastCollision : RaycastCollision {
        // 半径
        public float Radius { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="end">終了位置</param>
        /// <param name="radius">半径</param>
        public SphereRaycastCollision(Vector3 start, Vector3 end, float radius)
            : base(start, end) {
            Radius = radius;
        }

        /// <summary>
        /// 当たり判定実行
        /// </summary>
        /// <param name="layerMask">衝突対象のLayerMask</param>
        /// <param name="hitResults">判定格納用配列</param>
        /// <returns>衝突有効数</returns>
        protected override int HitCheck(int layerMask, RaycastHit[] hitResults) {
            var direction = End - Start;
            var distance = direction.magnitude;
            if (distance <= float.Epsilon) {
                return 0;
            }

            direction /= distance;
            return Physics.SphereCastNonAlloc(Start, Radius, direction, hitResults, distance, layerMask);
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        protected override void DrawGizmosInternal() {
            Gizmos.DrawWireSphere(Start, Radius);
            Gizmos.DrawWireSphere(End, Radius);
            var split = 8;
            var rot = Quaternion.LookRotation(End - Start);
            var deltaRot = Quaternion.Euler(0.0f, 0.0f, 360.0f / split);
            for (var i = 0; i < split; i++) {
                var up = rot * Vector3.up;
                Gizmos.DrawLine(Start + up * Radius, End + up * Radius);
                rot *= deltaRot;
            }
        }
    }
}