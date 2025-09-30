using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// レイヤータイプ
    /// </summary>
    public enum LayerType {
        Default,
        Push,
        Team0,
        Team1,
        Team2,
        Cutscene,
        Environment,
        Vfx,
    }
    
    /// <summary>
    /// レイヤーのユーティリティクラス
    /// </summary>
    public static class LayerUtility {
        private static readonly Dictionary<LayerType, int> Layers = new();

        /// <summary>
        /// レイヤーの取得
        /// </summary>
        public static int GetLayer(LayerType layerType) {
            if (Layers.TryGetValue(layerType, out var layer)) {
                return layer;
            }

            layer = LayerMask.NameToLayer(layerType.ToString());
            Layers[layerType] = layer;
            return layer;
        }

        /// <summary>
        /// レイヤーマスクの取得
        /// </summary>
        public static int GetLayerMask(params LayerType[] layerTypes) {
            return LayerMask.GetMask(layerTypes.Select(x => x.ToString()).ToArray());
        }
    }
}