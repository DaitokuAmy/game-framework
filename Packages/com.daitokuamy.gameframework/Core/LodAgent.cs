using System;

namespace GameFramework.Core {
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
                OnChangedLodLevel?.Invoke(_lodLevel);
            }
        }

        /// <summary>LodLevelの変更通知</summary>
        public event Action<int> OnChangedLodLevel;
    }
}