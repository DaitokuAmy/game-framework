using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewer用の背景初期化データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_environment_master_hoge.asset", menuName = "Sample Game/Model Viewer/Environment Master Data")]
    public class ModelViewerEnvironmentActorMasterData : ScriptableObject, IEnvironmentActorMaster {
        [Tooltip("表示名")]
        public string displayName;
        [Tooltip("背景読み込み用のアセットキー")]
        public string assetKey;
        [Tooltip("アクター配置ルート座標")]
        public Vector3 rootPosition;
        [Tooltip("アクター配置ルート向き")]
        public Vector3 rootAngles;
        
        string IEnvironmentActorMaster.DisplayName => string.IsNullOrEmpty(displayName) ? name.Replace("dat_model_viewer_environment_master_", "") : displayName;
        string IEnvironmentActorMaster.AssetKey => assetKey;
        Vector3 IEnvironmentActorMaster.RootPosition => rootPosition;
        Vector3 IEnvironmentActorMaster.RootAngles => rootAngles;
    }
}
