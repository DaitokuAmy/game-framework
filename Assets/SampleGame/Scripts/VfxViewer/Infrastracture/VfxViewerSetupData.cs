using System;
using UnityEngine;

namespace SampleGame.VfxViewer {
    /// <summary>
    /// VFXビューア用データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_vfx_viewer_setup.asset", menuName = "SampleGame/Vfx Viewer/Setup Data")]
    public class VfxViewerSetupData : ScriptableObject {
        [Header("Model")]
        [Tooltip("初期状態で読み込むVfxDataId")]
        public string defaultVfxDataId = "";
        [Tooltip("VfxDataのAssetKeyリスト")]
        public string[] vfxDataIds = Array.Empty<string>();

        [Header("Environment")]
        [Tooltip("初期状態で読み込む環境ID")]
        public string defaultEnvironmentId = "fld000";
        [Tooltip("EnvironmentIDリスト")]
        public string[] environmentIds = Array.Empty<string>();
    }
}
