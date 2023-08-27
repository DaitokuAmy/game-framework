using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// エイムターゲット用インターフェース
    /// </summary>
    public interface IAimTarget {
        /// <summary>ターゲットの半径</summary>
        float Radius { get; }
        
        /// <summary>
        /// エイムターゲットの座標を取得
        /// </summary>
        Vector3 GetPosition();
    }
}
