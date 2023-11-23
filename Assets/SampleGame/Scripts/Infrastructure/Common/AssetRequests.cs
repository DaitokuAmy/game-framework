using Opera.Infrastructure.Common;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure.Common {
    /// <summary>
    /// Master用のAssetRequest
    /// </summary>
    internal abstract class MasterAssetRequest<T> : GameFramework.AssetSystems.AssetRequest<T>
        where T : Object {
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase };
        public override string Address => $"Assets/SampleGame/MasterData/{RelativePath}";

        /// <summary>相対パス</summary>
        protected string RelativePath { get; set; } = "";
    }

    /// <summary>
    /// PlayerMasterData読み込み用リクエスト
    /// </summary>
    internal class PlayerMasterAssetRequest : MasterAssetRequest<PlayerMasterData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerMasterAssetRequest(int id) {
            RelativePath = $"Player/dat_master_player_{id:0000}.asset";
        }
    }
}