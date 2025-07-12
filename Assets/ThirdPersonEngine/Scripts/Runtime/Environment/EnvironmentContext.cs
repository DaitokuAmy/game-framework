using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 環境設定
    /// </summary>
    public class EnvironmentContext : IEnvironmentContext {
        public EnvironmentDefaultSettings DefaultSettings { get; set; }
        public float EnvironmentSaturate { get; set; }
        public float EnvironmentMultiply { get; set; }
        public Light Sun { get; set; }
        public bool ChangeShadowColorByDistance { get; set; }
        public Color NearShadowColor { get; set; }
        public Color FarShadowColor { get; set; }
        public float ShadowColorChangeDistance { get; set; }
        public float ShadowColorBlendRange { get; set; }
    }
}