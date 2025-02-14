using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// LineRaycastコリジョン
    /// </summary>
    public class LineRaycastCollision : RaycastCollision {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="end">終了位置</param>
        public LineRaycastCollision(Vector3 start, Vector3 end)
            : base(start, end) {
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
            return Physics.RaycastNonAlloc(Start, direction, hitResults, distance, layerMask);
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        protected override void DrawGizmosInternal() {
            Gizmos.DrawLine(Start, End);
        }
    }
}