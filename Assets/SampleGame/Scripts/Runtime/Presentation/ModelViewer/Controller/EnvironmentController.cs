using System.Linq;
using GameFramework.LogicSystems;
using SampleGame.Domain.ModelViewer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// 背景制御用のController
    /// </summary>
    public class EnvironmentController : Logic, IEnvironmentController {
        private readonly IReadOnlyEnvironmentModel _model;
        private readonly Scene _scene;
        private Light _directionalLight;

        /// <summary>ディレクショナルライトのY角度</summary>
        float IEnvironmentController.LightAngleY => _directionalLight != null ? _directionalLight.transform.eulerAngles.y : 0.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentController(IReadOnlyEnvironmentModel model, Scene scene) {
            _model = model;
            _scene = scene;
            _directionalLight = FindDirectionalLight(scene);
        }

        /// <summary>
        /// ディレクショナルライトのY角度を設定
        /// </summary>
        void IEnvironmentController.SetLightAngleY(float angleY) {
            if (_directionalLight == null) {
                return;
            }

            var angles = _directionalLight.transform.eulerAngles;
            angles.y = angleY;
            _directionalLight.transform.eulerAngles = angles;
        }

        /// <summary>
        /// 平行光源の検索
        /// </summary>
        private Light FindDirectionalLight(Scene scene) {
            if (!scene.IsValid()) {
                return null;
            }
            
            foreach (var rootObj in scene.GetRootGameObjects()) {
                var light = rootObj.GetComponentsInChildren<Light>()
                    .FirstOrDefault(x => x.type == LightType.Directional && x.bakingOutput.lightmapBakeType != LightmapBakeType.Baked);
                if (light == null) {
                    continue;
                }

                return light;
            }

            return null;
        }
    }
}