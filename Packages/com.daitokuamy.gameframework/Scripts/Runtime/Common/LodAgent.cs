using System;

namespace GameFramework {
    /// <summary>
    /// Lod提供用クラス
    /// </summary>
    public class LodAgent : ILodProvider {
        private int _lodLevel;

        /// <summary>現在のLodレベル</summary>
        public int LodLevel {
            get => _lodLevel;
            set {
                if (_lodLevel == value) {
                    return;
                }
                
                _lodLevel = value;
                ChangedLodLevelEvent?.Invoke(_lodLevel);
            }
        }

        /// <summary>LodLevelの変更通知</summary>
        public event Action<int> ChangedLodLevelEvent;
    }
}