using UnityEngine;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// プレビュー用のVfxデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_preview_vfx_setup_hoge.asset", menuName = "SampleGame/Vfx Viewer/Preview Vfx Setup Data")]
    public class PreviewVfxSetupData : ScriptableObject {
        [Tooltip("VFXのタイプ")]
        public VfxType vfxType;
        [Tooltip("VFXのPrefab")]
        public GameObject prefab;
    }
}
