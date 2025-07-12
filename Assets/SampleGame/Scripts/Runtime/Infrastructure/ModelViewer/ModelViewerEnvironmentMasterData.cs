using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewer用の背景初期化データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_environment_master_hoge.asset", menuName = "SampleGame/Model Viewer/Environment Master Data")]
    public class ModelViewerEnvironmentMasterData : ScriptableObject, IEnvironmentMaster {
        [Tooltip("表示名")]
        public string displayName;
        [Tooltip("背景読み込み用のアセットキー")]
        public string assetKey;
        [Tooltip("アクター配置ルート座標")]
        public Vector3 rootPosition;
        [Tooltip("アクター配置ルート向き")]
        public Vector3 rootAngles;
        
        string IEnvironmentMaster.DisplayName => string.IsNullOrEmpty(displayName) ? name.Replace("dat_model_viewer_environment_master_", "") : displayName;
        string IEnvironmentMaster.AssetKey => assetKey;
        Vector3 IEnvironmentMaster.RootPosition => rootPosition;
        Vector3 IEnvironmentMaster.RootAngles => rootAngles;
    }
}
