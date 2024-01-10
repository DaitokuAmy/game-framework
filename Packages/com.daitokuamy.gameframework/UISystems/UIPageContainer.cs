using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// 切り替えて遷移し続けるUIScreenのコンテナ
    /// </summary>
    public class UIPageContainer : UIScreenContainer {
        private string _currentKey;
        private List<string> _stackKeys = new();

        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="childKey">遷移予定のChildを表すキー</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle Transition(string childKey, IUITransition transition = null, bool immediate = false) {
            var op = new AsyncOperator();
            var nextChildScreen = FindChild(childKey);

            if (_currentKey == childKey) {
                op.Completed();
                return op;
            }

            if (transition == null) {
                transition = new CrossUITransition();
            }
            
            // スタックに存在する場合は戻り遷移扱い
            var backIndex =_stackKeys.IndexOf(childKey);
            var back = backIndex >= 0;
            
            // 間にあるStackをクリアする
            if (back) {
                for (var i = _stackKeys.Count - 1; i >= backIndex; i--) {
                    _stackKeys.RemoveAt(i);
                }
            }
            // null遷移はStackをクリアする
            else if (string.IsNullOrEmpty(childKey)) {
                _stackKeys.Clear();
            }
            
            // 並び順変更
            SetAsLastSibling(childKey);

            // 遷移処理
            var prevUIScreen = FindChild(_currentKey)?.uiScreen;
            var nextUIScreen = nextChildScreen?.uiScreen;
            _currentKey = childKey;
            if (nextUIScreen != null) {
                _stackKeys.Add(_currentKey);
            }
            
            StartCoroutine(transition.TransitRoutine(this, prevUIScreen, nextUIScreen, back ? TransitionType.Back : TransitionType.Forward, immediate),
                () => op.Completed(),
                () => op.Aborted(),
                err => op.Aborted(err));

            return op;
        }

        /// <summary>
        /// 戻り遷移処理
        /// </summary>
        /// <param name="transition">遷移方法</param>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle Back(IUITransition transition = null, bool immediate = false) {
            var backKey = _stackKeys.Count > 1 ? _stackKeys[_stackKeys.Count - 2] : null;
            return Transition(backKey, transition, immediate);
        }

        /// <summary>
        /// 全部閉じる処理
        /// </summary>
        /// <param name="transition">遷移方法</param>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle Clear(IUITransition transition = null, bool immediate = false) {
            return Transition(null, transition, immediate);
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