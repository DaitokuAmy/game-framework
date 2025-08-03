using System.Collections;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// 閉じてから開く遷移処理
    /// </summary>
    public class OutInTransition : ITransition {
        private readonly bool _closeImmediate;
        private readonly bool _openImmediate;
        private readonly ThreadPriority _backgroundThreadPriority;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="closeImmediate">閉じるアニメーションを即時に行うか</param>
        /// <param name="openImmediate">開くアニメーションを即時に行うか</param>
        /// <param name="backgroundThreadPriority">遷移時のバックグラウンドスレッド優先度</param>
        public OutInTransition(bool closeImmediate = false, bool openImmediate = false, ThreadPriority backgroundThreadPriority = ThreadPriority.Normal) {
            _closeImmediate = closeImmediate;
            _openImmediate = openImmediate;
            _backgroundThreadPriority = backgroundThreadPriority;
        }
        
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="resolver">遷移処理解決者</param>
        /// <param name="immediate">即時遷移か</param>
        IEnumerator ITransition.TransitionRoutine(ITransitionResolver resolver, bool immediate) {
            resolver.Start();

            // 非アクティブ
            resolver.DeactivatePrev();

            // エフェクト開始＆閉じる
            if (_closeImmediate) {
                yield return resolver.EnterEffectRoutine();
                yield return resolver.ClosePrevRoutine(true);
            }
            else {
                yield return new MergedCoroutine(resolver.EnterEffectRoutine(), resolver.ClosePrevRoutine(immediate));
            }

            // BGスレッド優先度の変更
            var prevThreadPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = _backgroundThreadPriority;

            // 解放
            yield return resolver.UnloadPrevRoutine();

            // 読み込み
            yield return resolver.LoadNextRoutine();
            
            // BGスレッド優先度を戻す
            Application.backgroundLoadingPriority = prevThreadPriority;

            // エフェクト終了＆開く
            if (_openImmediate) {
                yield return resolver.OpenNextRoutine(true);
                yield return resolver.ExitEffectRoutine();
            }
            else {
                yield return new MergedCoroutine(resolver.ExitEffectRoutine(), resolver.OpenNextRoutine(immediate));
            }

            // アクティブ化
            resolver.ActivateNext();

            resolver.Finish();
        }
    }
}