using System;
using UnityEngine;

namespace GameFramework.RendererSystems {
    /// <summary>
    /// Hdr指定可能なColor
    /// </summary>
    [Serializable]
    public struct HdrColor {
        public Color color;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HdrColor(Color color) {
            this.color = color;
        }

        /// <summary>
        /// Color型からの暗黙変換
        /// </summary>
        public static implicit operator HdrColor(Color color) {
            return new HdrColor(color);
        }

        /// <summary>
        /// Color型への暗黙変換
        /// </summary>
        public static implicit operator Color(HdrColor hdrColor) {
            return hdrColor.color;
        }
    }
}