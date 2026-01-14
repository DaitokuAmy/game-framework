using Cysharp.Threading.Tasks;

namespace SampleGame.Application {
    /// <summary>
    /// BattleのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface IAppNavigator {
        /// <summary>
        /// バトルへの遷移
        /// </summary>
        UniTask TransitionToBattle();
        
        /// <summary>
        /// バトル中ポーズへの遷移
        /// </summary>
        UniTask TransitionToBattlePause();
    }
}
