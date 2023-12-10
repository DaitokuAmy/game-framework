using GameFramework.ModelSystems;

namespace SampleGame.Domain.Common {
    /// <summary>
    /// PlayerModelの読み取り用インターフェース
    /// </summary>
    public interface IReadOnlyPlayerModel {
        /// <summary>マスターデータ参照</summary>
        IPlayerMasterData MasterData { get; }
        /// <summary>名前</summary>
        string Name { get; }
        /// <summary>Prefabのアセットキー</summary>
        string PrefabAssetKey { get; }
    }
    
    /// <summary>
    /// ユーザー操作対象のプレイヤーキャラモデル
    /// </summary>
    public class PlayerModel : IdModel<int, PlayerModel>, IReadOnlyPlayerModel {
        /// <summary>マスターデータ参照</summary>
        public IPlayerMasterData MasterData { get; private set; }
        /// <summary>名前</summary>
        public string Name => MasterData?.Name ?? "";
        /// <summary>Prefabのアセットキー</summary>
        public string PrefabAssetKey => MasterData?.PrefabAssetKey ?? "";
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected PlayerModel(int id) : base(id) {
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Setup(IPlayerMasterData masterData) {
            MasterData = masterData;
        }
    }
}
