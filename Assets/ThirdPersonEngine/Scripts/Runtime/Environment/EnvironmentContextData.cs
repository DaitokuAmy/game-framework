using GameFramework.EnvironmentSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 環境設定用のアセット
    /// </summary>
    [CreateAssetMenu(fileName = "dat_env_fld000.asset", menuName = "ThirdPersonEngine/Environment/Context Data")]
    public class EnvironmentContextData : ScriptableObject {
        [SerializeField]
        public EnvironmentDefaultSettings defaultSettings;
        [SerializeField]
        public float environmentSaturate = 1.0f;
        [SerializeField]
        public float environmentMultiply = 1.0f;
        [Header("Shadow Color Settings")]
        [SerializeField]
        public bool changeShadowColorByDistance = false;
        [SerializeField]
        public Color nearShadowColor = Color.gray;
        [SerializeField]
        public Color farShadowColor = Color.gray;
        [SerializeField]
        public float shadowColorChangeDistance = 50.0f;
        [SerializeField]
        public float shadowColorBlendRange = 10.0f;

        /// <summary>
        /// コンテキストの作成
        /// </summary>
        public EnvironmentContext CreateContext() {
            return new EnvironmentContext {
                DefaultSettings = defaultSettings,
                EnvironmentSaturate = environmentSaturate,
                EnvironmentMultiply = environmentMultiply,
                ChangeShadowColorByDistance = changeShadowColorByDistance,
                NearShadowColor = nearShadowColor,
                FarShadowColor = farShadowColor,
                ShadowColorChangeDistance = shadowColorChangeDistance,
                ShadowColorBlendRange = shadowColorBlendRange
            };
        }
    }
}