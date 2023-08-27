using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// エイミング制御用インターフェース
    /// </summary>
    public interface IAimableActor {
        /// <summary>
        /// エイミング
        /// </summary>
        void Aim(Vector3 targetPoint);
    }
}
