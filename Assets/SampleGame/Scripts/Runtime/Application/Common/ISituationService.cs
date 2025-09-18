using GameFramework.Core;

namespace SampleGame.Application {
    /// <summary>
    /// Situation遷移に関するサービスインターフェース
    /// </summary>
    public partial interface ISituationService {
        /// <summary>
        /// 戻る処理
        /// </summary>
        IProcess Back(int depth = 1, bool cross = false);
    }
}
