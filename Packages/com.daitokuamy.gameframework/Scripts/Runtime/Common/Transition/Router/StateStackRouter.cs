using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GameFramework {
    /// <summary>
    /// Stack管理用StateRouter
    /// </summary>
    public class StateStackRouter<TKey, TState, TOption> : IStateRouter<TKey, TState, TOption>
        where TState : class {
        private readonly IStateContainer<TKey, TState, TOption> _stateContainer;
        private readonly List<TKey> _stack = new();
        private readonly string _label;

        private bool _disposed;

        /// <inheritdoc/>
        string IMonitoredStateRouter.Label => _label;
        /// <summary>戻り先の情報</summary>
        string IMonitoredStateRouter.BackStateInfo {
            get {
                if (_stack.Count <= 1) {
                    return "None";
                }

                var backKey = _stack[^2];
                return backKey.ToString();
            }
        }

        /// <inheritdoc/>
        public TState Current => _stateContainer.Current;
        /// <inheritdoc/>
        public TKey CurrentKey => _stack.Count > 0 ? _stack[^1] : default;
        /// <inheritdoc/>
        public bool IsTransitioning => _stateContainer.IsTransitioning;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StateStackRouter(IStateContainer<TKey, TState, TOption> container, string label = "", [CallerFilePath] string caller = "") {
            _stateContainer = container;
            _label = string.IsNullOrEmpty(label) ? PathUtility.GetRelativePath(caller) : label;
            StateMonitor.AddRouter(this);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            StateMonitor.RemoveRouter(this);

            _stack.Clear();
        }

        /// <summary>
        /// モニタリング用の詳細情報取得
        /// </summary>
        void IMonitoredStateRouter.GetDetails(List<(string label, string text)> lines) {
            // Stack情報の返却
            for (var i = 0; i < _stack.Count; i++) {
                lines.Add((i == 0 ? "Stack" : "", _stack[i].ToString()));
            }
        }

        /// <inheritdoc/>
        TState[] IStateRouter<TKey, TState, TOption>.GetStates() {
            return _stateContainer.GetStates();
        }

        /// <summary>
        /// スタックのクリア
        /// </summary>
        public void ClearStack() {
            _stack.Clear();
        }

        /// <summary>
        /// スタックのセット
        /// </summary>
        public void SetStack(IReadOnlyList<TKey> stack) {
            _stack.Clear();
            for (var i = 0; i < stack.Count; i++) {
                if (_stateContainer.FindState(stack[i]) == null) {
                    continue;
                }

                if (_stack.Contains(stack[i])) {
                    continue;
                }

                _stack.Add(stack[i]);
            }
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="key">遷移ターゲットを決めるキー</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="step">終了ステップ</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<TState> Transition(TKey key, TOption option = default, TransitionStep step = TransitionStep.Complete, Action<TState> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects) {
            return TransitionInternal(key, option, false, step, setupAction, transition, effects);
        }

        /// <summary>
        /// 戻り遷移処理
        /// </summary>
        /// <param name="depth">戻り階層数(1～)</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<TState> Back(int depth = 1, TOption option = default, Action<TState> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects) {
            // 深さのクランプ
            if (depth > _stack.Count - 1) {
                depth = _stack.Count - 1;
            }

            if (_stack.Count <= 1 || depth <= 0) {
                return TransitionHandle<TState>.Empty;
            }

            // 戻り先を見つける
            var backKey = _stack[_stack.Count - 1 - depth];

            // 戻り遷移
            return TransitionInternal(backKey, option, true, TransitionStep.Complete, setupAction, transition, effects);
        }

        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<TState> Reset(Action<TState> setupAction = null, params ITransitionEffect[] effects) {
            return ResetInternal(setupAction, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="key">遷移ターゲットを決めるキー</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="back">戻りか</param>
        /// <param name="step">終了ステップ</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        private TransitionHandle<TState> TransitionInternal(TKey key, TOption option, bool back, TransitionStep step, Action<TState> setupAction, ITransition transition, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                return new TransitionHandle<TState>(new Exception("In transitioning"));
            }

            // 同じ場所なら何もしない
            if (key.Equals(CurrentKey)) {
                return TransitionHandle<TState>.Empty;
            }

            // NextNodeがない
            if (_stateContainer.FindState(key) == null) {
                return new TransitionHandle<TState>(new Exception($"Next key is not found. key:{key}"));
            }

            // スタックの中に含まれていたらそこまでスタックを戻す
            var foundIndex = _stack.IndexOf(key);
            if (foundIndex >= 0) {
                _stack.RemoveRange(foundIndex, _stack.Count - foundIndex);
            }

            // スタックに追加
            _stack.Add(key);

            // 遷移実行
            return _stateContainer.Transition(key, option, back, step, setupAction, transition, effects);
        }

        /// <summary>
        /// 現在のStateをリセットする
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="effects">遷移時演出</param>
        private TransitionHandle<TState> ResetInternal(Action<TState> setupAction, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                return new TransitionHandle<TState>(new Exception("In transitioning"));
            }

            if (Current == null) {
                return TransitionHandle<TState>.Empty;
            }

            // リセット実行
            return _stateContainer.Reset(setupAction, effects);
        }
    }
}