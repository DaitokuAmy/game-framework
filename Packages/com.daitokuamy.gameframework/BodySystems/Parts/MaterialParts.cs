using System;
using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
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
            public string key;
            [Tooltip("対象のMaterial")]
            public RendererMaterial rendererMaterial;

            public bool IsValid => rendererMaterial.IsValid;
            public Renderer Renderer => rendererMaterial.renderer;
            public int MaterialIndex => rendererMaterial.materialIndex;
        }

        [Tooltip("マテリアル登録情報")]
        public Info[] infos;
    }
}