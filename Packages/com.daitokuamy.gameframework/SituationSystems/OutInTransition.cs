using System.Collections;
using GameFramework.CoroutineSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 閉じてから開く遷移処理
    /// </summary>
    public class OutInTransition : ITransition {
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="resolver">遷移処理解決者</param>
        IEnumerator ITransition.TransitRoutine(ITransitionResolver resolver) {
            resolver.Start();

            // 非アクティブ
            resolver.DeactivatePrev();

            // エフェクト開始＆閉じる
            yield return new MergedCoroutine(resolver.EnterEffectRoutine(), resolver.ClosePrevRoutine());

            // 解放
            yield return resolver.UnloadPrevRoutine();

            // 読み込み
            yield return resolver.LoadNextRoutine();

            // エフェクト終了＆開く
            yield return new MergedCoroutine(resolver.ExitEffectRoutine(), resolver.OpenNextRoutine());

            // アクティブ化
            resolver.ActivateNext();

            resolver.Finish();
        }
    }
}