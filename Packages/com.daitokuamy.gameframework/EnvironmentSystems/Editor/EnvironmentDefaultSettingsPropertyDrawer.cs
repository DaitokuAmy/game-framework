using System.Linq;
using GameFramework.EnvironmentSystems;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using RenderSettings = UnityEngine.RenderSettings;

namespace GameFramework.EnvironmentSystems.Editor {
    /// <summary>
    /// 環境設定のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(EnvironmentDefaultSettings))]
    public class EnvironmentDefaultSettingsPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// Style情報
        /// </summary>
        private static class Styles {
            public static readonly GUIContent SkyTitle = EditorGUIUtility.TrTextContent("Sky");
            public static readonly GUIContent AmbientTitle = EditorGUIUtility.TrTextContent("Environment Lighting");

            public static readonly GUIContent SubtractiveShadowColor = EditorGUIUtility.TrTextContent(
                "Realtime Shadow Color",
                "The color used for mixing realtime shadows with baked lightmaps in Subtractive lighting mode. The color defines the darkest point of the realtime shadow.");

            public static readonly GUIContent AmbientSkyColor = EditorGUIUtility.TrTextContent("Sky Color",
                "Controls the color of light emitted from the sky in the Scene.");

            public static readonly GUIContent AmbientEquatorColor = EditorGUIUtility.TrTextContent("Equator Color",
                "Controls the color of light emitted from the sides of the Scene.");

            public static readonly GUIContent AmbientGroundColor = EditorGUIUtility.TrTextContent("Ground Color",
                "Controls the color of light emitted from the ground of the Scene.");

            public static readonly GUIContent AmbientColor = EditorGUIUtility.TrTextContent("Ambient Color",
                "Controls the color of the ambient light contributed to the Scene.");

            public static readonly GUIContent AmbientSource = EditorGUIUtility.TrTextContent("Source",
                "Specifies whether to use a skybox, gradient, or color for ambient light contributed to the Scene.");

            public static readonly GUIContent AmbientIntensity = EditorGUIUtility.TrTextContent("Intensity Multiplier",
                "Controls the brightness of the skybox lighting in the Scene.");

            public static readonly GUIContent SkyboxMaterial = EditorGUIUtility.TrTextContent("Skybox Material",
                "Specifies the material that is used to simulate the sky or other distant background in the Scene.");

            public static readonly GUIContent SkyboxWarning =
                EditorGUIUtility.TrTextContent("Shader of this material does not support skybox rendering.");

            public static readonly GUIContent ReflectionTitle =
                EditorGUIUtility.TrTextContent("Environment Reflections");

            public static readonly GUIContent ReflectionMode = EditorGUIUtility.TrTextContent("Source",
                "Specifies whether to use the skybox or a custom cube map for reflection effects in the Scene.");

            public static readonly GUIContent ReflectionResolution = EditorGUIUtility.TrTextContent("Resolution",
                "Controls the resolution for the cube map assigned to the skybox material for reflection effects in the Scene.");

            public static readonly GUIContent CustomReflection = EditorGUIUtility.TrTextContent("Cubemap",
                "Specifies the custom cube map used for reflection effects in the Scene.");

            public static readonly GUIContent ReflectionIntensity =
                EditorGUIUtility.TrTextContent("Intensity Multiplier",
                    "Controls how much the skybox or custom cubemap affects reflections in the Scene. A value of 1 produces physically correct results.");

            public static readonly GUIContent ReflectionBounces = EditorGUIUtility.TrTextContent("Bounces",
                "Controls how many times a reflection includes other reflections. A value of 1 results in the Scene being rendered once so mirrored reflections will be black. A value of 2 results in mirrored reflections being visible in the Scene.");

            public static readonly GUIContent FogTitle = EditorGUIUtility.TrTextContent("Fog");

            public static readonly GUIContent Fog =
                EditorGUIUtility.TrTextContent("Enabled", "Specifies whether fog is used in the Scene or not.");

            public static readonly GUIContent FogColor =
                EditorGUIUtility.TrTextContent("Color", "Controls the color of the fog drawn in the Scene.");

            public static readonly GUIContent FogMode = EditorGUIUtility.TrTextContent("Mode",
                "Controls the mathematical function determining the way fog accumulates with distance from the camera. Options are Linear, Exponential, and Exponential Squared.");

            public static readonly GUIContent FogDensity = EditorGUIUtility.TrTextContent("Density",
                "Controls the density of the fog effect in the Scene when using Exponential or Exponential Squared modes.");

            public static readonly GUIContent FogStartDistance = EditorGUIUtility.TrTextContent("Start",
                "Controls the distance from the camera where the fog will start in the Scene.");

            public static readonly GUIContent FogEndDistance = EditorGUIUtility.TrTextContent("End",
                "Controls the distance from the camera where the fog will completely obscure objects in the Scene.");

            public static readonly GUIContent OtherTitle = EditorGUIUtility.TrTextContent("Other Settings");

            public static readonly GUIContent HaloStrength = EditorGUIUtility.TrTextContent("Halo Strength",
                "Controls the visibility of the halo effect around lights in the Scene.");

            public static readonly GUIContent FlareStrength = EditorGUIUtility.TrTextContent("Flare Strength",
                "Controls the visibility of lens flares from lights in the Scene.");

            public static readonly GUIContent FlareFadeSpeed = EditorGUIUtility.TrTextContent("Flare Fade Speed",
                "Controls the time over which lens flares fade from view after initially appearing.");

            public static readonly int[] AmbientSourceValues = {
                (int)AmbientMode.Skybox,
                (int)AmbientMode.Trilight,
                (int)AmbientMode.Flat
            };

            public static readonly GUIContent[] AmbientSourceLabels = {
                EditorGUIUtility.TrTextContent("Skybox"),
                EditorGUIUtility.TrTextContent("Gradient"),
                EditorGUIUtility.TrTextContent("Color"),
            };

            public static readonly int[] ReflectionResolutionValues = {
                16,
                32,
                64,
                128,
                256,
                512,
                1024,
                2048,
            };

            public static readonly GUIContent[] ReflectionResolutionLabels =
                ReflectionResolutionValues.Select(x => new GUIContent(x.ToString())).ToArray();
        }

        private bool _foldout;
        private SerializedProperty _subtractiveShadowColor;
        private SerializedProperty _ambientMode;
        private SerializedProperty _ambientSkyColor;
        private SerializedProperty _ambientEquatorColor;
        private SerializedProperty _ambientGroundColor;
        private SerializedProperty _ambientIntensity;
        private SerializedProperty _skyboxMaterial;
        private SerializedProperty _defaultReflectionMode;
        private SerializedProperty _defaultReflectionResolution;
        private SerializedProperty _customReflection;
        private SerializedProperty _reflectionIntensity;
        private SerializedProperty _reflectionBounces;
        private SerializedProperty _fog;
        private SerializedProperty _fogColor;
        private SerializedProperty _fogMode;
        private SerializedProperty _fogDensity;
        private SerializedProperty _fogStartDistance;
        private SerializedProperty _fogEndDistance;
        private SerializedProperty _haloStrength;
        private SerializedProperty _flareStrength;
        private SerializedProperty _flareFadeSpeed;

        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GetProperties(property);

            void ColorField(Rect r, SerializedProperty p, GUIContent l) {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = p.hasMultipleDifferentValues;
                var n = EditorGUI.ColorField(r, l, p.colorValue, true, false, true);
                if (EditorGUI.EndChangeCheck()) {
                    p.colorValue = n;
                }

                EditorGUI.showMixedValue = false;
            }

            var skyboxMaterial = _skyboxMaterial.objectReferenceValue as Material;
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var lineOffsetUnit = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            var space = 10;
            var rect = position;
            rect.height = lineHeight;

            _foldout = EditorGUI.Foldout(rect, _foldout, label);
            rect.y += lineOffsetUnit;

            if (_foldout) {
                EditorGUI.indentLevel++;

                // Sky
                EditorGUI.LabelField(rect, Styles.SkyTitle, EditorStyles.boldLabel);
                rect.y += lineOffsetUnit;
                EditorGUI.indentLevel++;

                EditorGUI.showMixedValue = _skyboxMaterial.hasMultipleDifferentValues;
                EditorGUI.PropertyField(rect, _skyboxMaterial, Styles.SkyboxMaterial);
                rect.y += lineOffsetUnit;
                if (skyboxMaterial != null && !EditorMaterialUtility.IsBackgroundMaterial(skyboxMaterial)) {
                    rect.height = lineHeight * 2;
                    EditorGUI.HelpBox(rect, Styles.SkyboxWarning.text, MessageType.Warning);
                    rect.y += lineOffsetUnit + lineHeight;
                    rect.height = lineHeight;
                }

                EditorGUI.showMixedValue = _subtractiveShadowColor.hasMultipleDifferentValues;
                EditorGUI.PropertyField(rect, _subtractiveShadowColor, Styles.SubtractiveShadowColor);
                rect.y += lineOffsetUnit;
                EditorGUI.indentLevel--;

                rect.y += space;

                // Ambient
                EditorGUI.LabelField(rect, Styles.AmbientTitle, EditorStyles.boldLabel);
                rect.y += lineOffsetUnit;
                EditorGUI.indentLevel++;

                EditorGUI.showMixedValue = _ambientMode.hasMultipleDifferentValues;
                EditorGUI.IntPopup(rect, _ambientMode, Styles.AmbientSourceLabels, Styles.AmbientSourceValues,
                    Styles.AmbientSource);
                rect.y += lineOffsetUnit;
                switch ((AmbientMode)_ambientMode.intValue) {
                    case AmbientMode.Trilight: {
                        ColorField(rect, _ambientSkyColor, Styles.AmbientSkyColor);
                        rect.y += lineOffsetUnit;
                        ColorField(rect, _ambientEquatorColor, Styles.AmbientEquatorColor);
                        rect.y += lineOffsetUnit;
                        ColorField(rect, _ambientGroundColor, Styles.AmbientGroundColor);
                        rect.y += lineOffsetUnit;
                    }
                        break;

                    case AmbientMode.Flat: {
                        ColorField(rect, _ambientSkyColor, Styles.AmbientColor);
                        rect.y += lineOffsetUnit;
                    }
                        break;

                    case AmbientMode.Skybox:
                        if (skyboxMaterial == null) {
                            ColorField(rect, _ambientSkyColor, Styles.AmbientColor);
                            rect.y += lineOffsetUnit;
                        }
                        else {
                            EditorGUI.showMixedValue = _ambientIntensity.hasMultipleDifferentValues;
                            EditorGUI.Slider(rect, _ambientIntensity, 0.0f, 8.0f, Styles.AmbientIntensity);
                            rect.y += lineOffsetUnit;
                        }

                        break;
                }

                EditorGUI.indentLevel--;

                rect.y += space;

                // Reflection
                EditorGUI.LabelField(rect, Styles.ReflectionTitle, EditorStyles.boldLabel);
                rect.y += lineOffsetUnit;
                EditorGUI.indentLevel++;

                EditorGUI.showMixedValue = _defaultReflectionMode.hasMultipleDifferentValues;
                EditorGUI.PropertyField(rect, _defaultReflectionMode, Styles.ReflectionMode);
                rect.y += lineOffsetUnit;

                var defReflectionMode = (DefaultReflectionMode)_defaultReflectionMode.intValue;
                switch (defReflectionMode) {
                    case DefaultReflectionMode.Skybox:
                        EditorGUI.showMixedValue = _defaultReflectionResolution.hasMultipleDifferentValues;
                        EditorGUI.IntPopup(rect, _defaultReflectionResolution, Styles.ReflectionResolutionLabels,
                            Styles.ReflectionResolutionValues, Styles.ReflectionResolution);
                        rect.y += lineOffsetUnit;
                        break;

                    case DefaultReflectionMode.Custom:
                        EditorGUI.showMixedValue = _customReflection.hasMultipleDifferentValues;
                        EditorGUI.PropertyField(rect, _customReflection, Styles.CustomReflection);
                        rect.y += lineOffsetUnit;
                        break;
                }

                EditorGUI.showMixedValue = _reflectionIntensity.hasMultipleDifferentValues;
                EditorGUI.Slider(rect, _reflectionIntensity, 0.0f, 1.0f, Styles.ReflectionIntensity);
                rect.y += lineOffsetUnit;
                EditorGUI.showMixedValue = _reflectionBounces.hasMultipleDifferentValues;
                EditorGUI.IntSlider(rect, _reflectionBounces, 1, 5, Styles.ReflectionBounces);
                rect.y += lineOffsetUnit;

                EditorGUI.indentLevel--;

                rect.y += space;

                // Fog
                EditorGUI.LabelField(rect, Styles.FogTitle, EditorStyles.boldLabel);
                rect.y += lineOffsetUnit;
                EditorGUI.indentLevel++;

                EditorGUI.showMixedValue = _fog.hasMultipleDifferentValues;
                EditorGUI.PropertyField(rect, _fog, Styles.Fog);
                rect.y += lineOffsetUnit;
                if (_fog.boolValue) {
                    EditorGUI.indentLevel++;

                    EditorGUI.showMixedValue = _fogColor.hasMultipleDifferentValues;
                    EditorGUI.PropertyField(rect, _fogColor, Styles.FogColor);
                    rect.y += lineOffsetUnit;
                    EditorGUI.showMixedValue = _fogMode.hasMultipleDifferentValues;
                    EditorGUI.PropertyField(rect, _fogMode, Styles.FogMode);
                    rect.y += lineOffsetUnit;

                    if ((FogMode)_fogMode.intValue != FogMode.Linear) {
                        EditorGUI.showMixedValue = _fogDensity.hasMultipleDifferentValues;
                        EditorGUI.PropertyField(rect, _fogDensity, Styles.FogDensity);
                        rect.y += lineOffsetUnit;
                    }
                    else {
                        EditorGUI.showMixedValue = _fogStartDistance.hasMultipleDifferentValues;
                        EditorGUI.PropertyField(rect, _fogStartDistance, Styles.FogStartDistance);
                        rect.y += lineOffsetUnit;
                        EditorGUI.showMixedValue = _fogEndDistance.hasMultipleDifferentValues;
                        EditorGUI.PropertyField(rect, _fogEndDistance, Styles.FogEndDistance);
                        rect.y += lineOffsetUnit;
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                rect.y += space;

                // Other
                EditorGUI.LabelField(rect, Styles.OtherTitle, EditorStyles.boldLabel);
                rect.y += lineOffsetUnit;
                EditorGUI.indentLevel++;

                EditorGUI.showMixedValue = _haloStrength.hasMultipleDifferentValues;
                EditorGUI.Slider(rect, _haloStrength, 0.0f, 1.0f, Styles.HaloStrength);
                rect.y += lineOffsetUnit;
                EditorGUI.showMixedValue = _flareFadeSpeed.hasMultipleDifferentValues;
                EditorGUI.PropertyField(rect, _flareFadeSpeed, Styles.FlareFadeSpeed);
                rect.y += lineOffsetUnit;
                EditorGUI.showMixedValue = _flareStrength.hasMultipleDifferentValues;
                EditorGUI.Slider(rect, _flareStrength, 0.0f, 1.0f, Styles.FlareStrength);
                rect.y += lineOffsetUnit;

                EditorGUI.indentLevel--;

                rect.y += space;

                // 値の取得/反映ボタン
                rect.width = position.width * 0.5f;
                if (GUI.Button(rect, "Get Settings")) {
                    _subtractiveShadowColor.colorValue = RenderSettings.subtractiveShadowColor;
                    _ambientMode.intValue = (int)RenderSettings.ambientMode;
                    _ambientSkyColor.colorValue = RenderSettings.ambientSkyColor;
                    _ambientEquatorColor.colorValue = RenderSettings.ambientEquatorColor;
                    _ambientGroundColor.colorValue = RenderSettings.ambientGroundColor;
                    _ambientIntensity.floatValue = RenderSettings.ambientIntensity;
                    _skyboxMaterial.objectReferenceValue = RenderSettings.skybox;
                    _defaultReflectionMode.intValue = (int)RenderSettings.defaultReflectionMode;
                    _defaultReflectionResolution.intValue = RenderSettings.defaultReflectionResolution;
                    _customReflection.objectReferenceValue = RenderSettings.customReflection;
                    _reflectionIntensity.floatValue = RenderSettings.reflectionIntensity;
                    _reflectionBounces.intValue = RenderSettings.reflectionBounces;
                    _fog.boolValue = RenderSettings.fog;
                    _fogColor.colorValue = RenderSettings.fogColor;
                    _fogMode.intValue = (int)RenderSettings.fogMode;
                    _fogDensity.floatValue = RenderSettings.fogDensity;
                    _fogStartDistance.floatValue = RenderSettings.fogStartDistance;
                    _fogEndDistance.floatValue = RenderSettings.fogEndDistance;
                    _haloStrength.floatValue = RenderSettings.haloStrength;
                    _flareStrength.floatValue = RenderSettings.flareStrength;
                    _flareFadeSpeed.floatValue = RenderSettings.flareFadeSpeed;
                }

                rect.x += rect.width;
                GUI.enabled = property.serializedObject.targetObjects.Length == 1;
                if (GUI.Button(rect, "Set Settings")) {
                    RenderSettings.subtractiveShadowColor = _subtractiveShadowColor.colorValue;
                    RenderSettings.ambientMode = (AmbientMode)_ambientMode.intValue;
                    RenderSettings.ambientSkyColor = _ambientSkyColor.colorValue;
                    RenderSettings.ambientEquatorColor = _ambientEquatorColor.colorValue;
                    RenderSettings.ambientGroundColor = _ambientGroundColor.colorValue;
                    RenderSettings.ambientIntensity = _ambientIntensity.floatValue;
                    RenderSettings.skybox = _skyboxMaterial.objectReferenceValue as Material;
                    RenderSettings.defaultReflectionMode = (DefaultReflectionMode)_defaultReflectionMode.intValue;
                    RenderSettings.defaultReflectionResolution = _defaultReflectionResolution.intValue;
                    RenderSettings.customReflection = _customReflection.objectReferenceValue as Cubemap;
                    RenderSettings.reflectionIntensity = _reflectionIntensity.floatValue;
                    RenderSettings.reflectionBounces = _reflectionBounces.intValue;
                    RenderSettings.fog = _fog.boolValue;
                    RenderSettings.fogColor = _fogColor.colorValue;
                    RenderSettings.fogMode = (FogMode)_fogMode.intValue;
                    RenderSettings.fogDensity = _fogDensity.floatValue;
                    RenderSettings.fogStartDistance = _fogStartDistance.floatValue;
                    RenderSettings.fogEndDistance = _fogEndDistance.floatValue;
                    RenderSettings.haloStrength = _haloStrength.floatValue;
                    RenderSettings.flareStrength = _flareStrength.floatValue;
                    RenderSettings.flareFadeSpeed = _flareFadeSpeed.floatValue;
                }

                GUI.enabled = true;

                rect.x = position.x;
                rect.width = position.width;
                rect.y += lineOffsetUnit;

                EditorGUI.showMixedValue = false;

                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// プロパティの高さ取得
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            GetProperties(property);

            var skyboxMaterial = _skyboxMaterial.objectReferenceValue as Material;
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var lineOffsetUnit = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            var space = 10;
            var totalHeight = 0.0f;

            totalHeight += lineOffsetUnit;

            if (!_foldout) {
                return totalHeight;
            }

            // Sky
            totalHeight += lineOffsetUnit * 2;

            if (skyboxMaterial != null && !EditorMaterialUtility.IsBackgroundMaterial(skyboxMaterial)) {
                totalHeight += lineOffsetUnit + lineHeight;
            }

            totalHeight += lineOffsetUnit * 2;

            totalHeight += space;

            // Ambient
            totalHeight += lineOffsetUnit * 2;
            switch ((AmbientMode)_ambientMode.intValue) {
                case AmbientMode.Trilight:
                    totalHeight += lineOffsetUnit * 3;
                    break;

                case AmbientMode.Flat:
                    totalHeight += lineOffsetUnit;
                    break;

                case AmbientMode.Skybox:
                    totalHeight += lineOffsetUnit;
                    break;
            }

            totalHeight += space;

            // Reflection
            totalHeight += lineOffsetUnit * 5;

            totalHeight += space;

            // Fog
            totalHeight += lineOffsetUnit * 2;

            if (_fog.boolValue) {
                totalHeight += lineOffsetUnit * 2;

                if ((FogMode)_fogMode.intValue != FogMode.Linear) {
                    totalHeight += lineOffsetUnit;
                }
                else {
                    totalHeight += lineOffsetUnit * 2;
                }
            }

            totalHeight += space;

            // Other
            totalHeight += lineOffsetUnit * 4;

            totalHeight += space;

            // Button
            totalHeight += lineOffsetUnit;

            return totalHeight;
        }

        /// <summary>
        /// プロパティの取得
        /// </summary>
        private void GetProperties(SerializedProperty property) {
            _subtractiveShadowColor = property.FindPropertyRelative("subtractiveShadowColor");
            _ambientMode = property.FindPropertyRelative("ambientMode");
            _ambientSkyColor = property.FindPropertyRelative("ambientSkyColor");
            _ambientEquatorColor = property.FindPropertyRelative("ambientEquatorColor");
            _ambientGroundColor = property.FindPropertyRelative("ambientGroundColor");
            _ambientIntensity = property.FindPropertyRelative("ambientIntensity");
            _skyboxMaterial = property.FindPropertyRelative("skyboxMaterial");
            _defaultReflectionMode = property.FindPropertyRelative("defaultReflectionMode");
            _defaultReflectionResolution = property.FindPropertyRelative("defaultReflectionResolution");
            _customReflection = property.FindPropertyRelative("customReflection");
            _reflectionIntensity = property.FindPropertyRelative("reflectionIntensity");
            _reflectionBounces = property.FindPropertyRelative("reflectionBounces");
            _fog = property.FindPropertyRelative("fog");
            _fogColor = property.FindPropertyRelative("fogColor");
            _fogMode = property.FindPropertyRelative("fogMode");
            _fogDensity = property.FindPropertyRelative("fogDensity");
            _fogStartDistance = property.FindPropertyRelative("fogStartDistance");
            _fogEndDistance = property.FindPropertyRelative("fogEndDistance");
            _haloStrength = property.FindPropertyRelative("haloStrength");
            _flareStrength = property.FindPropertyRelative("flareStrength");
            _flareFadeSpeed = property.FindPropertyRelative("flareFadeSpeed");
        }
    }
}