using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// スタック管理をしないただ切り替えるだけのUIScreenのコンテナ
    /// </summary>
    public class UISheetContainer : UIScreenContainer {
        private string _currentKey;

        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="childKey">遷移予定のChildを表すキー</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="transitionType">遷移タイプ</param>
        /// <param name="immediate">即時遷移するか</param>
        /// <param name="force">同じキーだとしても開きなおすか</param>
        /// <param name="initAction">初期化アクション</param>
        public AsyncOperationHandle<UIScreen> Change(string childKey, IUITransition transition = null, TransitionType transitionType = TransitionType.Forward, bool immediate = false, bool force = false, Action<UIScreen> initAction = null) {
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
                    transition = new OutInUITransition();
                }
                else {
                    transition = new CrossUITransition();
                }
            }

            // 並び順変更
            SetAsLastSibling(childKey);

            // 遷移処理
            var prevUIScreen = FindChild(_currentKey)?.uiScreen;
            var nextUIScreen = nextChildScreen?.uiScreen;
            _currentKey = childKey;

            StartCoroutine(transition.TransitRoutine(this, prevUIScreen, nextUIScreen, transitionType, immediate, initAction),
                () => op.Completed(nextUIScreen),
                () => op.Aborted(),
                err => op.Aborted(err));

            return op;
        }

        /// <summary>
        /// 中身をクリアする
        /// </summary>
        /// <param name="transition">遷移方法</param>
        /// <param name="transitionType">遷移タイプ</param>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle Clear(IUITransition transition = null, TransitionType transitionType = TransitionType.Forward, bool immediate = false) {
            var op = new AsyncOperator();
            var handle = Change(null, transition, transitionType, immediate);
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

        /// <summary>
        /// 開く処理（後処理）
        /// </summary>
        protected override void PostOpen(TransitionType transitionType, bool immediate) {
            base.PostOpen(transitionType, immediate);

            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                childView.uiScreen.OpenAsync(transitionType, true);
            }
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected override IEnumerator CloseRoutine(TransitionType transitionType, IScope cancelScope) {
            yield return base.CloseRoutine(transitionType, cancelScope);

            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                yield return childView.uiScreen.CloseAsync(transitionType, false);
            }
        }

        /// <summary>
        /// 閉じる処理（後処理）
        /// </summary>
        protected override void PostClose(TransitionType transitionType, bool immediate) {
            base.PostClose(transitionType, immediate);

            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                childView.uiScreen.CloseAsync(transitionType, true);
            }
        }
    }
}