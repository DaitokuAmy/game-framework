using System.Collections;
using GameFramework.CoroutineSystems;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// 閉じてから開く遷移処理
    /// </summary>
    public class OutInTransition : ITransition {
        private bool _closeImmediate;
        private bool _openImmediate;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="closeImmediate">閉じるアニメーションを即時に行うか</param>
        /// <param name="openImmediate">開くアニメーションを即時に行うか</param>
        public OutInTransition(bool closeImmediate = false, bool openImmediate = false) {
            _closeImmediate = closeImmediate;
            _openImmediate = openImmediate;
        }
        
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="resolver">遷移処理解決者</param>
        IEnumerator ITransition.TransitRoutine(ITransitionResolver resolver) {
            resolver.Start();

            // 非アクティブ
            resolver.DeactivatePrev();

            // エフェクト開始＆閉じる
            if (_closeImmediate) {
                yield return resolver.EnterEffectRoutine();
                yield return resolver.ClosePrevRoutine(true);
            }
            else {
                yield return new MergedCoroutine(resolver.EnterEffectRoutine(), resolver.ClosePrevRoutine());
            }

            // 解放
            yield return resolver.UnloadPrevRoutine();

            // 読み込み
            yield return resolver.LoadNextRoutine();

            // エフェクト終了＆開く
            if (_openImmediate) {
                yield return resolver.OpenNextRoutine(true);
                yield return resolver.ExitEffectRoutine();
            }
            else {
                yield return new MergedCoroutine(resolver.ExitEffectRoutine(), resolver.OpenNextRoutine());
            }

            // アクティブ化
            resolver.ActivateNext();

            resolver.Finish();
        }
    }
}