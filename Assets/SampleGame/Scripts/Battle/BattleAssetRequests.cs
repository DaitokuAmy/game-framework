using UnityEngine.SceneManagement;

namespace SampleGame.Battle {
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