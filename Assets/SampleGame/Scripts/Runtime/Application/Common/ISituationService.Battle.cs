using GameFramework.Core;

namespace SampleGame.Application {
    /// <summary>
    /// BattleのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface ISituationService {
        /// <summary>
        /// バトルへの遷移
        /// </summary>
        IProcess TransitionBattle();
        
        /// <summary>
        /// バトル中ポーズへの遷移
        /// </summary>
        IProcess TransitionBattlePause();
    }
}
