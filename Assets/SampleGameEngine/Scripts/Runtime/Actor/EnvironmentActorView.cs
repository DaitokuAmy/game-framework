using System.Linq;
using GameFramework.ActorSystem;
using UnityEngine;

namespace SampleGameEngine.ModelViewer {
    /// <summary>
    /// 環境用のアクター
    /// </summary>
    public class EnvironmentActorView : ActorView {
        /// <summary>ライト用のSlot</summary>
        public Transform LightSlot { get; }
        /// <summary>配置ルートのSlot</summary>
        public Transform RootSlot { get; }
        /// <summary>平行光源</summary>
        public Light DirectionalLight { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentActorView(Body body)
            : base(body) {
            LightSlot = Body.Locators["LightSlot"];
            RootSlot = Body.Locators["Root"];
            DirectionalLight = Body.GetComponentsInChildren<Light>()
                .FirstOrDefault(x => x.type == LightType.Directional && x.bakingOutput.lightmapBakeType != LightmapBakeType.Baked);
        }
    }
}