using GameFramework.AssetSystem;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// キャラプレファブの読み込み要求
    /// </summary>
    public readonly struct CharacterPrefabAssetRequest : IAssetRequest<GameObject> {
        /// <inheritdoc/>
        public string Address { get; }
        /// <inheritdoc/>
        public bool IsValid => !string.IsNullOrEmpty(Address);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterPrefabAssetRequest(string assetKey) {
            Address = $"Assets/SampleGame/RemoteAssets/Actor/Character/{GetBoneId(assetKey)}/pfb_act_{assetKey}.prefab";
        }

        /// <summary>
        /// 骨IDを取得
        /// </summary>
        private static string GetBoneId(string assetKey) {
            return assetKey.Split("_")[0];
        }
    }

    /// <summary>
    /// フィールドシーンの読み込み要求
    /// </summary>
    public readonly struct FieldSceneRequest : ISceneRequest {
        /// <inheritdoc/>
        public string Address { get; }
        /// <inheritdoc/>
        public bool ActivateOnLoad { get; }
        /// <inheritdoc/>
        public bool IsValid => !string.IsNullOrEmpty(Address);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FieldSceneRequest(string assetKey, bool activateOnLoad = true) {
            Address = $"Assets/SampleGame/RemoteAssets/Environment/Field/{assetKey}/scn_env_{assetKey}.unity";
            ActivateOnLoad = activateOnLoad;
        }
    }
}
