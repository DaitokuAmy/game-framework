using System;
using System.Collections.Generic;
using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// モデルビューア用マスターデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_master.asset", menuName = "Sample Game/Model Viewer/Master Data")]
    public class ModelViewerMasterData : ScriptableObject, IModelViewerMaster {
        [Tooltip("初期状態で読み込むアクターアセットキーのIndex")]
        public int defaultActorAssetKeyIndex = 0;
        [Tooltip("ActorDataのAssetKeyリスト")]
        public string[] actorAssetKeys = Array.Empty<string>();
        [Tooltip("初期状態で読み込む環境アセットキーのIndex")]
        public int defaultEnvironmentAssetKeyIndex = 0;
        [Tooltip("EnvironmentIDリスト")]
        public string[] environmentAssetKeys = Array.Empty<string>();

        int IModelViewerMaster.DefaultActorAssetKeyIndex => defaultActorAssetKeyIndex;
        IReadOnlyList<string> IModelViewerMaster.ActorAssetKeys => actorAssetKeys;
        int IModelViewerMaster.DefaultEnvironmentAssetKeyIndex => defaultEnvironmentAssetKeyIndex;
        IReadOnlyList<string> IModelViewerMaster.EnvironmentAssetKeys => environmentAssetKeys;
    }
}