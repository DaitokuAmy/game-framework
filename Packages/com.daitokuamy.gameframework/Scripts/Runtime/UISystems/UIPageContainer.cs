using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using System;

namespace GameFramework.UISystems {
    /// <summary>
    /// 切り替えて遷移し続けるUIScreenのコンテナ
    /// </summary>
    public class UIPageContainer : UIScreenContainer {
        private readonly List<string> _stackKeys = new();
        
        private string _currentKey;

        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="childKey">遷移予定のChildを表すキー</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="immediate">即時遷移するか</param>
        /// <param name="force">同じキーだとしても開きなおすか</param>
        /// <param name="initAction">初期化アクション</param>
        /// <param name="effects">遷移エフェクトリスト</param>
        public AsyncOperationHandle<UIScreen> Transition(string childKey, ITransition transition = null, bool immediate = false, bool force = false, Action<UIScreen> initAction = null, params ITransitionEffect[] effects) {
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
            var prevScreen = FindChild(_currentKey)?.uiScreen;
            var nextScreen = nextChildScreen?.uiScreen;
            _currentKey = childKey;
            if (nextScreen != null) {
                _stackKeys.Add(_currentKey);
            }

            // 遷移開始
            StartTransition(transition, prevScreen, nextScreen, back ? TransitionDirection.Back : TransitionDirection.Forward, immediate, effects, initAction, op);
            return op;
        }

        /// <summary>
        /// 戻り遷移処理
        /// </summary>
        /// <param name="transition">遷移方法</param>
        /// <param name="immediate">即時遷移するか</param>
        /// <param name="initAction">初期化アクション</param>
        /// <param name="effects">遷移エフェクトリスト</param>
        public AsyncOperationHandle<UIScreen> Back(ITransition transition = null, bool immediate = false, Action<UIScreen> initAction = null, params ITransitionEffect[] effects) {
            var backKey = _stackKeys.Count > 1 ? _stackKeys[_stackKeys.Count - 2] : null;
            return Transition(backKey, transition, immediate, true, initAction);
        }

        /// <summary>
        /// 全部閉じる処理
        /// </summary>
        /// <param name="transition">遷移方法</param>
        /// <param name="immediate">即時遷移するか</param>
        public AsyncOperationHandle Clear(ITransition transition = null, bool immediate = false) {
            var op = new AsyncOperator();
            var handle = Transition(null, transition, immediate);
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
        protected override void PostOpen(TransitionDirection transitionDirection, bool immediate) {
            base.PostOpen(transitionDirection, immediate);

            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                childView.uiScreen.OpenAsync(transitionDirection, true);
            }
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected override IEnumerator CloseRoutine(TransitionDirection transitionDirection, IScope cancelScope) {
            yield return base.CloseRoutine(transitionDirection, cancelScope);
            
            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                yield return childView.uiScreen.CloseAsync(transitionDirection, false);
            }
        }

        /// <summary>
        /// 閉じる処理（後処理）
        /// </summary>
        protected override void PostClose(TransitionDirection transitionDirection, bool immediate) {
            base.PostClose(transitionDirection, immediate);
            
            var childView = FindChild(_currentKey);
            if (childView != null && childView.uiScreen != null) {
                childView.uiScreen.CloseAsync(transitionDirection, true);
            }
        }
    }
}