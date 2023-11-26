using SampleGame.Infrastructure.Common;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// BattlePlayerMasterData読み込み用リクエスト
    /// </summary>
    internal class BattlePlayerMasterAssetRequest : MasterAssetRequest<BattlePlayerMasterData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerMasterAssetRequest(int id) {
            RelativePath = $"BattlePlayer/dat_master_battle_player_{id:0000}.asset";
        }
    }
}