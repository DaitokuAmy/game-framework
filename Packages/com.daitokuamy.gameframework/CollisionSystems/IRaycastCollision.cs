using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// レイキャストコリジョン用インターフェース
    /// </summary>
    public interface IRaycastCollision : IVisualizable {
        /// <summary>有効状態</summary>
        bool IsActive { get; set; }
        /// <summary>開始位置</summary>
        Vector3 Start { get; set; }
        /// <summary>終了位置</summary>
        Vector3 End { get; set; }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="layerMask">当たり判定の対象とするLayerMask</param>
        /// <param name="newHitResults">新しくヒットしたRaycastHit結果格納用リスト</param>
        bool Tick(int layerMask, List<RaycastHit> newHitResults);

        /// <summary>
        /// 現在のEndをStartにして、Endを進める
        /// </summary>
        /// <param name="nextEnd">新しいEnd</param>
        void March(Vector3 nextEnd);

        /// <summary>
        /// 衝突履歴のクリア
        /// </summary>
        void ClearHistory();
    }
}