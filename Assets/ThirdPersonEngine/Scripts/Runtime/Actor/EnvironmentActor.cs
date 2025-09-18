using System.Linq;
using GameFramework.ActorSystems;
using UnityEngine;

namespace ThirdPersonEngine.ModelViewer {
    /// <summary>
    /// 環境用のアクター
    /// </summary>
    public class EnvironmentActor : Actor {
        /// <summary>ライト用のSlot</summary>
        public Transform LightSlot => Body.Locators["LightSlot"];
        /// <summary>配置ルートのSlot</summary>
        public Transform RootSlot => Body.Locators["Root"];
        /// <summary>平行光源</summary>
        public Light DirectionalLight { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentActor(Body body) : base(body) {
            DirectionalLight = Body.GetComponentsInChildren<Light>()
                .FirstOrDefault(x => x.type == LightType.Directional && x.bakingOutput.lightmapBakeType != LightmapBakeType.Baked);
        }
    }
}