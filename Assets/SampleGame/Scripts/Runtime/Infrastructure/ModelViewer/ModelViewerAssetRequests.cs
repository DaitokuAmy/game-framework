using GameFramework.AssetSystem;
using SampleGameEngine;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewerConfigDataの読み込み要求
    /// </summary>
    public readonly struct ModelViewerConfigDataRequest : IAssetRequest<ModelViewerConfigData> {
        /// <summary>読み込み対象アドレス</summary>
        public string Address => "Assets/SampleGame/System/ModelViewer/Settings/dat_model_viewer_config.asset";
        /// <summary>有効な要求か</summary>
        public bool IsValid => true;
    }

    /// <summary>
    /// PreviewActorDataの読み込み要求
    /// </summary>
    public readonly struct PreviewActorDataRequest : IAssetRequest<PreviewActorData> {
        /// <summary>読み込み対象アドレス</summary>
        public string Address { get; }
        /// <summary>有効な要求か</summary>
        public bool IsValid => !string.IsNullOrEmpty(Address);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorDataRequest(string assetKey) {
            Address = $"Assets/SampleGame/RemoteAssets/Actor/PreviewActor/dat_act_preview_{assetKey}.asset";
        }
    }
}
