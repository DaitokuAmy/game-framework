using SampleGame.Domain.Battle;
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

    /// <summary>
    /// BattleCharacterActorSetupData読み込み用リクエスト
    /// </summary>
    internal class BattleCharacterActorSetupAssetRequest : ActorAssetRequest<BattleCharacterActorSetupData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActorSetupAssetRequest(string assetKey) {
            var actorId = assetKey.Substring(0, "pl000".Length);
            RelativePath = $"Battle/{actorId}/dat_battle_character_actor_setup_{assetKey}.asset";
        }
    }
}