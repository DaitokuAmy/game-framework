using System;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// Rendererに設定されているMaterialの参照管理
    /// </summary>
    [Serializable]
    public struct RendererMaterial {
        [Tooltip("対象のRenderer")]
        public Renderer renderer;
        [Tooltip("Rendererに保持されているMaterialのIndex")]
        public int materialIndex;

        /// <summary>有効なRendererMaterialか</summary>
        public bool IsValid =>
            renderer != null && materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length;
        /// <summary>CloneしたMaterial</summary>
        public Material Material => IsValid ? renderer.materials[materialIndex] : null;
        /// <summary>CloneしないMaterial</summary>
        public Material SharedMaterial => IsValid ? renderer.sharedMaterials[materialIndex] : null;
    }
}
