using System.Collections;
using GameFramework.CoroutineSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 閉じると開くを同時に行う遷移
    /// </summary>
    public class CrossTransition : ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="resolver">遷移処理解決者</param>
        IEnumerator ITransition.TransitRoutine(ITransitionResolver resolver) {
            resolver.Start();

            // エフェクト開始＆読み込み
            yield return new MergedCoroutine(resolver.EnterEffectRoutine(), resolver.LoadNextRoutine());

            // アクティブ化
            resolver.ActivateNext();

            // 閉じる＆開く＆エフェクト終了
            yield return new MergedCoroutine(resolver.ClosePrevRoutine(), resolver.OpenNextRoutine(),
                resolver.ExitEffectRoutine());

            // 非アクティブ化
            resolver.DeactivatePrev();

            // 解放
            yield return resolver.UnloadPrevRoutine();

            resolver.Finish();
        }
    }
}