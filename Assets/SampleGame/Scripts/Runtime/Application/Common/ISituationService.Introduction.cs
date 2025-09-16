using GameFramework.Core;

namespace SampleGame.Application {
    /// <summary>
    /// IntroductionのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface ISituationService {
        /// <summary>
        /// タイトルトップへの遷移
        /// </summary>
        IProcess TransitionTitleTop();
        
        /// <summary>
        /// タイトルオプションへの遷移
        /// </summary>
        IProcess TransitionTitleOption();
    }
}
