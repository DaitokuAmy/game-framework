using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョン用インターフェース
    /// </summary>
    public interface ICollision : IVisualizable {
        /// <summary>
        /// コリジョンの中央位置
        /// </summary>
        Vector3 Center { get; set; }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="layerMask">当たり判定の対象とするLayerMask</param>
        /// <param name="newColliderResults">新しくヒットしたCollider結果格納用リスト</param>
        bool Tick(int layerMask, List<Collider> newColliderResults);

        /// <summary>
        /// 衝突履歴のクリア
        /// </summary>
        void ClearHistory();
    }
}