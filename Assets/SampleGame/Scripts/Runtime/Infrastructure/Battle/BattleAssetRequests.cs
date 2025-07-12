using ThirdPersonEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// BattleCharacterActorSetupData用のAssetRequest
    /// </summary>
    public class BattleCharacterActorDataRequest : ActorAssetRequest<BattleCharacterActorData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActorDataRequest(string assetKey) : base($"BattleCharacter/{GetMainId(assetKey)}/Data/dat_battle_character_actor_setup_{assetKey}.asset") {
        }

        /// <summary>
        /// メインIDの取得
        /// </summary>
        private static string GetMainId(string assetKey) {
            return assetKey.Split("_")[0];
        }
    }
}