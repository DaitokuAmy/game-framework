using System;

namespace GameFramework {
    /// <summary>
    /// Lod情報を提供するためのインターフェース
    /// </summary>
    public interface ILodProvider {
        /// <summary>現在のLodレベル</summary>
        int LodLevel { get; }

        /// <summary>LodLevelの変更通知</summary>
        event Action<int> ChangedLodLevelEvent;
    }
}