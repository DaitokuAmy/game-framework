using System.Collections;

namespace GameFramework {
    /// <summary>
    /// 閉じると開くを同時に行う遷移
    /// </summary>
    public class CrossTransition : ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="resolver">遷移処理解決者</param>
        /// <param name="immediate">即時遷移か</param>
        IEnumerator ITransition.TransitionRoutine(ITransitionResolver resolver, bool immediate) {
            resolver.Start();

            // エフェクト開始＆読み込み
            yield return new MergedCoroutine(resolver.EnterEffectRoutine(), resolver.LoadNextRoutine());

            // アクティブ化
            resolver.ActivateNext();

            // 閉じる＆開く＆エフェクト終了
            yield return new MergedCoroutine(resolver.ClosePrevRoutine(immediate), resolver.OpenNextRoutine(immediate),
                resolver.ExitEffectRoutine());

            // 非アクティブ化
            resolver.DeactivatePrev();

            // 解放
            yield return resolver.UnloadPrevRoutine();

            resolver.Finish();
        }
    }
}