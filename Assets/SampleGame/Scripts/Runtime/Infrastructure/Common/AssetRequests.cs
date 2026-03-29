using GameFramework.AssetSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// UIプレファブの読み込み要求
    /// </summary>
    public readonly struct UIPrefabAssetRequest : IAssetRequest<GameObject> {
        /// <inheritdoc/>
        public string Address { get; }
        /// <inheritdoc/>
        public bool IsValid => !string.IsNullOrEmpty(Address);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UIPrefabAssetRequest(string assetKey) {
            Address = $"Assets/SampleGame/UI/Root/pfb_ui_{assetKey}.prefab";
        }
    }

    /// <summary>
    /// UIシーンの読み込み要求
    /// </summary>
    public readonly struct UISceneRequest : ISceneRequest {
        /// <inheritdoc/>
        public string Address { get; }
        /// <inheritdoc/>
        public bool ActivateOnLoad => false;
        /// <inheritdoc/>
        public bool IsValid => !string.IsNullOrEmpty(Address);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UISceneRequest(string assetKey) {
            Address = $"Assets/SampleGame/UI/Root/scn_ui_{assetKey}.unity";
        }
    }

    /// <summary>
    /// テーブルデータの読み込み要求
    /// </summary>
    public readonly struct TableDataRequest<TAsset> : IAssetRequest<TAsset>
        where TAsset : Object {
        /// <inheritdoc/>
        public string Address { get; }
        /// <inheritdoc/>
        public bool IsValid => !string.IsNullOrEmpty(Address);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TableDataRequest(string assetKey) {
            Address = $"Assets/SampleGame/System/Table/dat_{assetKey}_table.asset";
        }
    }
}
