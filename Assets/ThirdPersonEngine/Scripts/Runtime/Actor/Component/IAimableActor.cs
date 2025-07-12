using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// エイミング制御用インターフェース
    /// </summary>
    public interface IAimableActor {
        /// <summary>
        /// ターゲットのソート
        /// </summary>
        void OrderTargets(List<(Vector3 lookAtPosition, Vector3 followPosition)> targetInfos);
        
        /// <summary>
        /// エイミング
        /// </summary>
        void Aim(IReadOnlyList<(Vector3 lookAtPosition, Vector3 followPosition)> targetInfos, float deltaTime);
        
        /// <summary>
        /// エイミング(即時)
        /// </summary>
        void AimImmediate(IReadOnlyList<(Vector3 lookAtPosition, Vector3 followPosition)> targetInfos);
    }
}
