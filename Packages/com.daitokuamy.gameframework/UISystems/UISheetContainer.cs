using System.Collections;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// スタック管理をしないただのScreenを切り替えるコンテナ
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
        public AsyncOperationHandle Change(string childKey, IUiTransition transition = null, TransitionType transitionType = TransitionType.Forward, bool immediate = false) {
            var op = new AsyncOperator();
            var nextChildScreen = FindChild(childKey);

            if (_currentKey == childKey) {
                op.Completed();
                return op;
            }

            if (transition == null) {
                transition = new CrossUiTransition();
            }
            
            // 並び順変更
            SetAsLastSibling(childKey);

            // 遷移処理
            var prevUIScreen = FindChild(_currentKey)?.uiScreen;
            var nextUIScreen = nextChildScreen?.uiScreen;
            _currentKey = childKey;
            
            StartCoroutine(transition.TransitRoutine(this, prevUIScreen, nextUIScreen, transitionType, immediate),
                () => op.Completed(),
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
        public AsyncOperationHandle Clear(IUiTransition transition = null, TransitionType transitionType = TransitionType.Forward, bool immediate = false) {
            return Change(null, transition, transitionType, immediate);
        }

        /// <summary>
        /// 開く処理（後処理）
        /// </summary>
        protected override void PostOpen(TransitionType transitionType) {
            base.PostOpen(transitionType);

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
        protected override void PostClose(TransitionType transitionType) {
            base.PostClose(transitionType);
            
            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                childView.uiScreen.CloseAsync(transitionType, true);
            }
        }
    }
}