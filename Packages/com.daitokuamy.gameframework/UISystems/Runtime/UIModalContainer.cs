using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// 重ね合わせて開いていくUIScreenのコンテナ
    /// </summary>
    public class UIModalContainer : UIScreenContainer {
        private string _currentKey;
        private List<string> _stackKeys = new();

        /// <summary>
        /// 追加処理
        /// </summary>
        /// <param name="childKey">遷移予定のChildを表すキー</param>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle<UIScreen> Push(string childKey, bool immediate = false) {
            var op = new AsyncOperator<UIScreen>();
            var nextChildScreen = FindChild(childKey);

            if (nextChildScreen == null) {
                op.Completed(null);
                return op;
            }

            if (_currentKey == childKey) {
                op.Completed(nextChildScreen.uiScreen);
                return op;
            }
            
            // スタックに存在する場合はスタックから消す
            _stackKeys.Remove(childKey);
            
            // 並び順変更
            SetAsLastSibling(childKey);

            // 遷移処理
            var prevUIScreen = FindChild(_currentKey)?.uiScreen;
            var nextUIScreen = nextChildScreen.uiScreen;
            _currentKey = childKey;
            _stackKeys.Add(childKey);

            IEnumerator Routine() {
                if (prevUIScreen != null) {
                    prevUIScreen.Interactable = false;
                }
                
                yield return nextUIScreen.OpenAsync(TransitionType.Forward, immediate);

                nextUIScreen.Interactable = true;
            }
            
            StartCoroutine(Routine(),
                () => op.Completed(nextUIScreen),
                () => op.Aborted(),
                err => op.Aborted(err));

            return op;
        }

        /// <summary>
        /// 一つ閉じる処理
        /// </summary>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle<UIScreen> Pop(bool immediate = false) {
            var op = new AsyncOperator<UIScreen>();

            if (string.IsNullOrEmpty(_currentKey)) {
                op.Completed(null);
                return op;
            }
            
            var nextKey = _stackKeys.Count > 1 ? _stackKeys[_stackKeys.Count - 2] : null;
            var nextChildScreen = FindChild(nextKey);

            // 遷移処理
            var prevUIScreen = FindChild(_currentKey)?.uiScreen;
            var nextUIScreen = nextChildScreen?.uiScreen;
            _currentKey = nextKey;

            IEnumerator Routine() {
                if (prevUIScreen != null) {
                    prevUIScreen.Interactable = false;
                    yield return prevUIScreen.CloseAsync(TransitionType.Forward, immediate);
                }

                if (nextUIScreen != null) {
                    nextUIScreen.Interactable = true;
                }
            
                // 並び順変更
                SetAsLastSibling(nextKey);
            }
            
            StartCoroutine(Routine(),
                () => op.Completed(nextUIScreen),
                () => op.Aborted(),
                err => op.Aborted(err));

            return op;
        }

        /// <summary>
        /// 全部閉じる
        /// </summary>
        public AsyncOperationHandle Clear(bool immediate = false) {
            var op = new AsyncOperator();

            if (string.IsNullOrEmpty(_currentKey)) {
                op.Completed();
                return op;
            }

            // 遷移処理
            var prevUIScreen = FindChild(_currentKey)?.uiScreen;
            _currentKey = null;
            _stackKeys.Clear();

            IEnumerator Routine() {
                if (prevUIScreen != null) {
                    prevUIScreen.Interactable = false;
                    yield return prevUIScreen.CloseAsync(TransitionType.Forward, immediate);
                }
            }
            
            StartCoroutine(Routine(),
                () => op.Completed(),
                () => op.Aborted(),
                err => op.Aborted(err));

            return op;
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