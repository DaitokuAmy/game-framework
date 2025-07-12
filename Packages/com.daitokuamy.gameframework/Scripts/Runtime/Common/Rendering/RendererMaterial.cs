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

        // 有効なRendererMaterialか
        public bool IsValid =>
            renderer != null && materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length;
        // CloneしたMaterial
        public Material Material => IsValid ? renderer.materials[materialIndex] : null;
        // CloneしないMaterial
        public Material SharedMaterial => IsValid ? renderer.sharedMaterials[materialIndex] : null;
    }
}