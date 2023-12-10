using Opera.Infrastructure.Common;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure.Common {
    /// <summary>
    /// AssetProviderのタイプ
    /// </summary>
    public enum AssetProviderType {
        AssetDatabase,
        Resources,
    }
    
    /// <summary>
    /// AssetRequest
    /// </summary>
    internal abstract class AssetRequest<T> : GameFramework.AssetSystems.AssetRequest<T>
        where T : Object {
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase };
        public override string Address => $"Assets/SampleGame/{RelativePath}";

        /// <summary>相対パス</summary>
        protected string RelativePath { get; set; } = "";
    }
    
    /// <summary>
    /// Master用のAssetRequest
    /// </summary>
    internal abstract class MasterAssetRequest<T> : AssetRequest<T>
        where T : Object {
        public override string Address => $"Assets/SampleGame/MasterData/{RelativePath}";
    }

    /// <summary>
    /// ActorAssetのAssetRequest基底
    /// </summary>
    internal abstract class ActorAssetRequest<T> : AssetRequest<T>
        where T : Object {
        public override string Address => $"Assets/SampleGame/ActorAssets/{RelativePath}";
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