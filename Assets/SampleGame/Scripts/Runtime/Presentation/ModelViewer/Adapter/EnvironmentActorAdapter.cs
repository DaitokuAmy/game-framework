using System.Linq;
using GameFramework;
using GameFramework.ActorSystems;
using SampleGame.Domain.ModelViewer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// 背景制御用のAdapter
    /// </summary>
    public class EnvironmentActorAdapter : ActorEntityLogic, IEnvironmentActorPort {
        private readonly IReadOnlyEnvironmentModel _model;
        private readonly Scene _scene;
        private Light _directionalLight;

        /// <inheritdoc/>
        Transform IEnvironmentActorPort.LightSlot => _directionalLight.transform;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentActorAdapter(IReadOnlyEnvironmentModel model, Scene scene) {
            _model = model;
            _scene = scene;
            _directionalLight = FindDirectionalLight(scene);
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