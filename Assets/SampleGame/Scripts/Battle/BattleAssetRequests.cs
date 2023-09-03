using UnityEngine.SceneManagement;

namespace SampleGame.Battle {
    /// <summary>
    /// BattlePlayerMasterDataのAssetRequest
    /// </summary>
    public class BattlePlayerMasterGameAssetRequest : GameAssetRequest<BattlePlayerMasterData> {
        public override string Address { get; }
        
        public BattlePlayerMasterGameAssetRequest(string assetKey) {
            Address = GetPath($"Battle/BattlePlayerMaster/dat_battle_player_master_{assetKey}.asset");
        }
    }

    /// <summary>
    /// バトルイベントカットシーンの読み込みリクエスト
    /// </summary>
    public class BattleEventCutsceneAssetRequest : SceneAssetRequest {
        // 読み込みAddress
        public override string Address { get; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">001</param>
        public BattleEventCutsceneAssetRequest(string assetKey) : base(LoadSceneMode.Additive) {
            Address = GetProjectPath($"CutsceneAssets/BattleEvent/{assetKey}/cutscene_battle_event_{assetKey}.unity");
        }
    }
}