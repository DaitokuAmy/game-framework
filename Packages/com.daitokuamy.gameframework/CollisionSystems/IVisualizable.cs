using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョンビジュアライズ用インターフェース
    /// </summary>
    public interface IVisualizable {
        /// <summary>
        /// ギズモ描画
        /// </summary>
        void DrawGizmos();
    }
}