using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// スタック管理をしないただ切り替えるだけのUIScreenのコンテナ
    /// </summary>
    public class UISheetContainer : UIScreenContainer {
        private string _currentKey;

        /// <inheritdoc/>
        protected override void PostOpen(TransitionDirection transitionDirection, bool immediate) {
            base.PostOpen(transitionDirection, immediate);

            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                childView.uiScreen.OpenAsync(transitionDirection, true);
            }
        }

        /// <inheritdoc/>
        protected override IEnumerator CloseRoutine(TransitionDirection transitionDirection, IScope cancelScope) {
            yield return base.CloseRoutine(transitionDirection, cancelScope);

            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                yield return childView.uiScreen.CloseAsync(transitionDirection, false);
            }
        }

        /// <inheritdoc/>
        protected override void PostClose(TransitionDirection transitionDirection, bool immediate) {
            base.PostClose(transitionDirection, immediate);

            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                childView.uiScreen.CloseAsync(transitionDirection, true);
            }
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="childKey">遷移予定のChildを表すキー</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="transitionType">遷移タイプ</param>
        /// <param name="immediate">即時遷移するか</param>
        /// <param name="force">同じキーだとしても開きなおすか</param>
        /// <param name="initAction">初期化アクション</param>
        /// <param name="effects">遷移中エフェクトリスト</param>
        public AsyncOperationHandle<UIScreen> Change(string childKey, ITransition transition = null, TransitionDirection transitionDirection = TransitionDirection.Forward, bool immediate = false, bool force = false, Action<UIScreen> initAction = null, params ITransitionEffect[] effects) {
            if (IsTransitioning) {
                return AsyncOperationHandle<UIScreen>.CanceledHandle;
            }
            
            var op = new AsyncOperator<UIScreen>();
            var nextChildScreen = FindChild(childKey);

            if (_currentKey == childKey) {
                if (!force) {
                    op.Completed(nextChildScreen?.uiScreen);
                    return op;
                }
            }

            if (transition == null) {
                if (_currentKey == childKey) {
                    transition = new OutInTransition();
                }
                else {
                    transition = new CrossTransition();
                }
            }

            // 並び順変更
            SetAsLastSibling(childKey);

            // 遷移処理
            var prevScreen = FindChild(_currentKey)?.uiScreen;
            var nextScreen = nextChildScreen?.uiScreen;
            _currentKey = childKey;

            // 遷移開始
            StartTransition(transition, prevScreen, nextScreen, transitionDirection, immediate, effects, initAction, op);
            return op;
        }

        /// <summary>
        /// 中身をクリアする
        /// </summary>
        /// <param name="transition">遷移方法</param>
        /// <param name="transitionType">遷移タイプ</param>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle Clear(ITransition transition = null, TransitionDirection transitionDirection = TransitionDirection.Forward, bool immediate = false) {
            var op = new AsyncOperator();
            var handle = Change(null, transition, transitionDirection, immediate);
            if (handle.IsError) {
                op.Aborted(handle.Exception);
                return op;
            }

            if (handle.IsDone) {
                op.Completed();
                return op;
            }

            handle.ListenTo(_ => op.Completed(), ex => op.Aborted(ex));
            return op;
        }
    }
}