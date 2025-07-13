using ThirdPersonEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// BattleCharacterActorSetupData用のAssetRequest
    /// </summary>
    public class BattleCharacterActorDataRequest : ActorAssetRequest<BattleCharacterActorData> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActorDataRequest(string assetKey) : base($"Character/{GetMainId(assetKey)}/Settings/dat_act_{assetKey}_battle.asset") {
        }

        /// <summary>
        /// メインIDの取得
        /// </summary>
        private static string GetMainId(string assetKey) {
            return assetKey.Split("_")[0];
        }
    }
}