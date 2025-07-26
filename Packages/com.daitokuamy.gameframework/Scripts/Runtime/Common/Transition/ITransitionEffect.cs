using System.Collections;

namespace GameFramework {
    /// <summary>
    /// 遷移演出用インターフェース
    /// </summary>
    public interface ITransitionEffect {
        /// <summary>
        /// 遷移開始
        /// </summary>
        void BeginTransition();
        
        /// <summary>
        /// 演出開始ルーチン
        /// </summary>
        IEnumerator EnterEffectRoutine();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();

        /// <summary>
        /// 演出終了ルーチン
        /// </summary>
        IEnumerator ExitEffectRoutine();
        
        /// <summary>
        /// 遷移終了
        /// </summary>
        void EndTransition();
    }
}