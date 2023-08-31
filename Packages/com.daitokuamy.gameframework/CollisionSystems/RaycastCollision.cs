using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// レイキャストコリジョンクラスの基底
    /// </summary>
    public abstract class RaycastCollision : IRaycastCollision {
        // 結果格納最大数
        private const int ResultCountMax = 8;

        // 当たり判定受け取り用の配列
        private static readonly RaycastHit[] s_workResults = new RaycastHit[ResultCountMax];

        // 衝突済みのCollider
        private readonly List<Collider> _hitColliders = new List<Collider>();

        /// <summary>アクティブ状態</summary>
        public bool IsActive { get; set; } = true;
        /// <summary>開始位置</summary>
        public Vector3 Start { get; set; }
        /// <summary>終了位置</summary>
        public Vector3 End { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="end">終了位置</param>
        public RaycastCollision(Vector3 start, Vector3 end) {
            Start = start;
            End = end;
        }

        /// <summary>
        /// 衝突履歴のクリア
        /// </summary>
        public void ClearHistory() {
            _hitColliders.Clear();
        }

        /// <summary>
        /// 現在のEndをStartにして、Endを進める
        /// </summary>
        /// <param name="nextEnd">新しいEnd</param>
        public void March(Vector3 nextEnd) {
            Start = End;
            End = nextEnd;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        bool IRaycastCollision.Tick(int layerMask, List<RaycastHit> newHitResults) {
            if (!IsActive) {
                return false;
            }
            
            var count = HitCheck(layerMask, s_workResults);
            if (count <= 0) {
                return false;
            }

            // ヒストリー分は除外して結果を作成
            for (var i = 0; i < count; i++) {
                var result = s_workResults[i];
                if (_hitColliders.Contains(result.collider)) {
                    continue;
                }

                newHitResults.Add(result);

                // ヒストリーに追加
                _hitColliders.Add(result.collider);
            }

            return newHitResults.Count > 0;
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
        protected abstract int HitCheck(int layerMask, RaycastHit[] hitResults);

        /// <summary>
        /// ギズモ描画(Override用)
        /// </summary>
        protected virtual void DrawGizmosInternal() {
        }
    }
}