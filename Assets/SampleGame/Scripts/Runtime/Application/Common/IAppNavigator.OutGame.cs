using Cysharp.Threading.Tasks;
using GameFramework;

namespace SampleGame.Application {
    /// <summary>
    /// OutGameのSituation遷移に関するサービスインターフェース
    /// </summary>
    partial interface IAppNavigator {
        /// <summary>
        /// 出撃トップへの遷移
        /// </summary>
        UniTask TransitionToSortieTop();
        
        /// <summary>
        /// 出撃兵科選択への遷移
        /// </summary>
        UniTask TransitionToSortieRoleSelect();
        
        /// <summary>
        /// 出撃兵科情報への遷移
        /// </summary>
        UniTask TransitionToSortieRoleInformation();
        
        /// <summary>
        /// 出撃ミッション選択への遷移
        /// </summary>
        UniTask TransitionToSortieMissionSelect();
        
        /// <summary>
        /// 出撃難易度選択への遷移
        /// </summary>
        UniTask TransitionToSortieDifficultySelect();
    }
}