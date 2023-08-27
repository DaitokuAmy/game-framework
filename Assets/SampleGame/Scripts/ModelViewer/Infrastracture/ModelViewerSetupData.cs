using System;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// モデルビューア用データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_setup.asset", menuName = "SampleGame/Model Viewer/Setup Data")]
    public class ModelViewerSetupData : ScriptableObject {
        [Header("Model")]
        [Tooltip("初期状態で読み込むActorDataId")]
        public string defaultActorDataId = "";
        [Tooltip("ActorDataのAssetKeyリスト")]
        public string[] actorDataIds = Array.Empty<string>();

        [Header("Environment")]
        [Tooltip("初期状態で読み込む環境ID")]
        public string defaultEnvironmentId = "fld000";
        [Tooltip("EnvironmentIDリスト")]
        public string[] environmentIds = Array.Empty<string>();
    }
}
