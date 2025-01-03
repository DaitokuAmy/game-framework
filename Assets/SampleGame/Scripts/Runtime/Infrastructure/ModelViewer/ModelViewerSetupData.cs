using System;
using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用初期化データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_setup.asset", menuName = "SampleGame/Model Viewer/Setup Data")]
    public class ModelViewerSetupData : ScriptableObject, IModelViewerMaster {
        [Header("Model")]
        [Tooltip("初期状態で読み込むアクターアセットキーのIndex")]
        public int defaultActorAssetKeyIndex = 0;
        [Tooltip("ActorDataのAssetKeyリスト")]
        public string[] actorAssetKeys = Array.Empty<string>();

        [Header("Environment")]
        [Tooltip("初期状態で読み込む環境アセットキーのIndex")]
        public int defaultEnvironmentAssetKeyIndex = 0;
        [Tooltip("EnvironmentIDリスト")]
        public string[] environmentAssetKeys = Array.Empty<string>();

        int IModelViewerMaster.DefaultActorAssetKeyIndex => defaultActorAssetKeyIndex;
        string[] IModelViewerMaster.ActorAssetKeys => actorAssetKeys;
        int IModelViewerMaster.DefaultEnvironmentAssetKeyIndex => defaultEnvironmentAssetKeyIndex;
        string[] IModelViewerMaster.EnvironmentAssetKeys => environmentAssetKeys;
    }
}
