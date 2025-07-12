using System;
using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Material情報保持クラス
    /// </summary>
    [DisallowMultipleComponent]
    public class MaterialParts : MonoBehaviour {
        /// <summary>
        /// 登録情報
        /// </summary>
        [Serializable]
        public class Info {
            [Tooltip("Materialを取得する際のキー")]
            public string key = "";
            [Tooltip("マテリアルの制御タイプ")]
            public MaterialInstance.ControlType controlType = MaterialInstance.ControlType.Auto;
            [Tooltip("対象のMaterial")]
            public RendererMaterial[] rendererMaterials = Array.Empty<RendererMaterial>();
        }

        [Tooltip("マテリアル登録情報")]
        public Info[] infos;
    }
}