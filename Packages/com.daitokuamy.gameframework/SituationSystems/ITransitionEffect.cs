using System.Collections;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 遷移エフェクト用インターフェース
    /// </summary>
    public interface ITransitionEffect {
        /// <summary>
        /// 遷移開始
        /// </summary>
        void Begin();
        
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
        
        /// <summary>
        /// 遷移終了
        /// </summary>
        void End();
    }
}