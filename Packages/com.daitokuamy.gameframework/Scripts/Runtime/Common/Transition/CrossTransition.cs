using System.Collections;

namespace GameFramework {
    /// <summary>
    /// 閉じると開くを同時に行う遷移
    /// </summary>
    public class CrossTransition : ITransition {
        /// <inheritdoc/>
        IEnumerator ITransition.TransitionRoutine(ITransitionResolver resolver, bool immediate) {
            resolver.Start();

            // 非アクティブ化
            resolver.DeactivatePrev();

            // エフェクト開始＆読み込み
            yield return new MergedCoroutine(resolver.EnterEffectRoutine(), resolver.LoadNextRoutine());

            // 閉じる＆開く＆エフェクト終了
            yield return new MergedCoroutine(resolver.ClosePrevRoutine(immediate), resolver.OpenNextRoutine(immediate),
                resolver.ExitEffectRoutine());

            // 解放
            resolver.UnloadPrev();

            // アクティブ化
            resolver.ActivateNext();

            resolver.Finish();
        }
    }
}
