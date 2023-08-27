using System.Collections;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 遷移エフェクト用インターフェース
    /// </summary>
    public interface ITransitionEffect {
        /// <summary>
        /// 開始ルーチン
        /// </summary>
        IEnumerator EnterRoutine();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();

        /// <summary>
        /// 終了ルーチン
        /// </summary>
        IEnumerator ExitRoutine();
    }
}