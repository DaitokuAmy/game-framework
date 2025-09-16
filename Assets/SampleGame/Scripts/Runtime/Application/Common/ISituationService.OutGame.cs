using GameFramework.Core;

namespace SampleGame.Application {
    /// <summary>
    /// OutGameのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface ISituationService {
        /// <summary>
        /// 出撃トップへの遷移
        /// </summary>
        IProcess TransitionSortieTop();
        
        /// <summary>
        /// 出撃兵科選択への遷移
        /// </summary>
        IProcess TransitionSortieRoleSelect();
        
        /// <summary>
        /// 出撃兵科情報への遷移
        /// </summary>
        IProcess TransitionSortieRoleInformation();
        
        /// <summary>
        /// 出撃ミッション選択への遷移
        /// </summary>
        IProcess TransitionSortieMissionSelect();
        
        /// <summary>
        /// 出撃難易度選択への遷移
        /// </summary>
        IProcess TransitionSortieDifficultySelect();
    }
}