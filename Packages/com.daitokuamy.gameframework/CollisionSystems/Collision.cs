using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョンクラスの基底
    /// </summary>
    public abstract class Collision : ICollision {
        // 結果格納最大数
        private const int ResultCountMax = 8;

        // 当たり判定受け取り用の配列
        private static readonly Collider[] s_workResults = new Collider[ResultCountMax];

        // 衝突済みのCollider
        private readonly List<Collider> _hitColliders = new List<Collider>();

        // 中心座標
        public abstract Vector3 Center { get; set; }

        /// <summary>
        /// 衝突履歴のクリア
        /// </summary>
        public void ClearHistory() {
            _hitColliders.Clear();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        bool ICollision.Tick(int layerMask, List<Collider> newColliderResults) {
            var count = HitCheck(layerMask, s_workResults);
            if (count <= 0) {
                return false;
            }

            // ヒストリー分は除外して結果を作成
            for (var i = 0; i < count; i++) {
                var result = s_workResults[i];
                if (_hitColliders.Contains(result)) {
                    continue;
                }

                newColliderResults.Add(result);

                // ヒストリーに追加
                _hitColliders.Add(result);
            }

            return newColliderResults.Count > 0;
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        void IVisualizable.DrawGizmos() {
            var prevColor = Gizmos.color;
            Gizmos.color = Color.red;
            DrawGizmosInternal();
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// 当たり判定実行
        /// </summary>
        /// <param name="layerMask">衝突対象のLayerMask</param>
        /// <param name="hitResults">判定格納用配列</param>
        /// <returns>衝突有効数</returns>
        protected abstract int HitCheck(int layerMask, Collider[] hitResults);

        /// <summary>
        /// ギズモ描画(Override用)
        /// </summary>
        protected virtual void DrawGizmosInternal() {
        }
    }
}