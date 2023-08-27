using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// Unityにある環境設定
    /// </summary>
    [Serializable]
    public class EnvironmentDefaultSettings {
        public Color subtractiveShadowColor;
        public AmbientMode ambientMode;
        public Color ambientSkyColor;
        public Color ambientEquatorColor;
        public Color ambientGroundColor;
        public float ambientIntensity;
        public Material skyboxMaterial;
        public DefaultReflectionMode defaultReflectionMode;
        public int defaultReflectionResolution;
#if UNITY_2022_1_OR_NEWER
        public Texture customReflectionTexture;
#else
        public Texture customReflection;
#endif
        public float reflectionIntensity;
        public int reflectionBounces;

        public bool fog;
        public Color fogColor;
        public FogMode fogMode;
        public float fogDensity;
        public float fogStartDistance;
        public float fogEndDistance;
        public float haloStrength;
        public float flareStrength;
        public float flareFadeSpeed;

        /// <summary>
        /// クローンの作成
        /// </summary>
        public EnvironmentDefaultSettings Clone() {
            return new EnvironmentDefaultSettings {
                subtractiveShadowColor = subtractiveShadowColor,
                ambientMode = ambientMode,
                ambientSkyColor = ambientSkyColor,
                ambientEquatorColor = ambientEquatorColor,
                ambientGroundColor = ambientGroundColor,
                ambientIntensity = ambientIntensity,
                skyboxMaterial = skyboxMaterial,
                defaultReflectionMode = defaultReflectionMode,
                defaultReflectionResolution = defaultReflectionResolution,
#if UNITY_2022_1_OR_NEWER
                customReflectionTexture = customReflectionTexture,
#else
                customReflection = customReflection,
#endif
                reflectionIntensity = reflectionIntensity,
                reflectionBounces = reflectionBounces,
                fog = fog,
                fogColor = fogColor,
                fogMode = fogMode,
                fogDensity = fogDensity,
                fogStartDistance = fogStartDistance,
                fogEndDistance = fogEndDistance,
                haloStrength = haloStrength,
                flareStrength = flareStrength,
                flareFadeSpeed = flareFadeSpeed,
            };
        }

        /// <summary>
        /// 現在の値を取得
        /// </summary>
        /// <returns></returns>
        public static EnvironmentDefaultSettings GetCurrent() {
            return new EnvironmentDefaultSettings {
                subtractiveShadowColor = RenderSettings.subtractiveShadowColor,
                ambientMode = RenderSettings.ambientMode,
                ambientSkyColor = RenderSettings.ambientSkyColor,
                ambientEquatorColor = RenderSettings.ambientEquatorColor,
                ambientGroundColor = RenderSettings.ambientGroundColor,
                ambientIntensity = RenderSettings.ambientIntensity,
                skyboxMaterial = RenderSettings.skybox,
                defaultReflectionMode = RenderSettings.defaultReflectionMode,
                defaultReflectionResolution = RenderSettings.defaultReflectionResolution,
#if UNITY_2022_1_OR_NEWER
                customReflectionTexture = RenderSettings.customReflectionTexture,
#else
                customReflection = RenderSettings.customReflection,
#endif
                reflectionIntensity = RenderSettings.reflectionIntensity,
                reflectionBounces = RenderSettings.reflectionBounces,

                fog = RenderSettings.fog,
                fogColor = RenderSettings.fogColor,
                fogMode = RenderSettings.fogMode,
                fogDensity = RenderSettings.fogDensity,
                fogStartDistance = RenderSettings.fogStartDistance,
                fogEndDistance = RenderSettings.fogEndDistance,
                haloStrength = RenderSettings.haloStrength,
                flareStrength = RenderSettings.flareStrength,
                flareFadeSpeed = RenderSettings.flareFadeSpeed
            };
        }

        /// <summary>
        /// 値の反映
        /// </summary>
        public void Apply() {
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.skybox = skyboxMaterial;
            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
#if UNITY_2022_1_OR_NEWER
            RenderSettings.customReflectionTexture = customReflectionTexture;
#else
            RenderSettings.customReflection = customReflection;
#endif
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.reflectionBounces = reflectionBounces;

            RenderSettings.fog = fog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
            RenderSettings.haloStrength = haloStrength;
            RenderSettings.flareStrength = flareStrength;
            RenderSettings.flareFadeSpeed = flareFadeSpeed;
        }

        /// <summary>
        /// 値の補間（補間出来ない物はtargetを使用）
        /// </summary>
        /// <param name="target">補間対象</param>
        /// <param name="ratio">ブレンド率</param>
        public EnvironmentDefaultSettings Lerp(EnvironmentDefaultSettings target, float ratio) {
            var newSettings = target.Clone();
            newSettings.subtractiveShadowColor =
                Color.Lerp(subtractiveShadowColor, target.subtractiveShadowColor, ratio);
            newSettings.ambientSkyColor = Color.Lerp(ambientSkyColor, target.ambientSkyColor, ratio);
            newSettings.ambientEquatorColor = Color.Lerp(ambientEquatorColor, target.ambientEquatorColor, ratio);
            newSettings.ambientGroundColor = Color.Lerp(ambientGroundColor, target.ambientGroundColor, ratio);
            newSettings.ambientIntensity = Mathf.Lerp(ambientIntensity, target.ambientIntensity, ratio);
            newSettings.reflectionIntensity = Mathf.Lerp(reflectionIntensity, target.reflectionIntensity, ratio);

            newSettings.fogColor = Color.Lerp(fogColor, target.fogColor, ratio);
            newSettings.fogDensity = Mathf.Lerp(fogDensity, target.fogDensity, ratio);
            newSettings.fogStartDistance = Mathf.Lerp(fogStartDistance, target.fogStartDistance, ratio);
            newSettings.fogEndDistance = Mathf.Lerp(fogEndDistance, target.fogEndDistance, ratio);
            newSettings.haloStrength = Mathf.Lerp(haloStrength, target.haloStrength, ratio);
            newSettings.flareStrength = Mathf.Lerp(flareStrength, target.flareStrength, ratio);
            newSettings.flareFadeSpeed = Mathf.Lerp(flareFadeSpeed, target.flareFadeSpeed, ratio);
            return newSettings;
        }
    }
}