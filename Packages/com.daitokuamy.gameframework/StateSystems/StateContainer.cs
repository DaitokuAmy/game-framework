using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.StateSystems {
    /// <summary>
    /// 状態制御クラス
    /// </summary>
    public class StateContainer<TState, TKey> : IDisposable
        where TState : IState<TKey>
        where TKey : IComparable {
        // Stateリスト
        private readonly Dictionary<TKey, TState> _states = new();
        // State用のScope
        private readonly DisposableScope _scope = new();
        // スタック
        private readonly List<TKey> _stack = new();

        // 状態変更通知(Prev > Next)
        public event Action<TKey, TKey> OnChangedState;

        // 現在のステートキー
        public TKey CurrentKey => _stack.Count > 0 ? _stack[_stack.Count - 1] : InvalidKey;
        // 無効キー
        public TKey InvalidKey { get; private set; }
        // 遷移予定のステートキー
        public TKey NextKey { get; private set; }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            Cleanup();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Setup(TKey invalidKey, params TState[] states) {
            Cleanup();

            InvalidKey = invalidKey;
            NextKey = invalidKey;

            foreach (var state in states) {
                // 無効キーは登録しない
                if (state.Key.Equals(invalidKey)) {
                    continue;
                }

                if (_states.ContainsKey(state.Key)) {
                    Debug.LogError($"Already exists state key. {state.Key}");
                    continue;
                }

                _states[state.Key] = state;
            }
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Cleanup() {
            Change(InvalidKey, true);

            _states.Clear();
            _stack.Clear();
            InvalidKey = default;
            NextKey = default;
        }

        /// <summary>
        /// Stateの変更
        /// </summary>
        /// <param name="key">Stateキー</param>
        /// <param name="immediate">即時変更するか</param>
        public void Change(TKey key, bool immediate = false) {
            if (key.Equals(NextKey)) {
                return;
            }

            // 遷移先登録
            NextKey = key;

            // 即時反映する場合、更新を実行
            if (immediate) {
                Update(0.0f);
            }
        }

        /// <summary>
        /// Stateを戻る
        /// </summary>
        /// <param name="immediate"></param>
        public bool Back(bool immediate = false) {
            if (_stack.Count <= 0) {
                return false;
            }
            
            // ひとつ前を遷移先にする
            var nextKey = _stack.Count > 1 ? _stack[_stack.Count - 2] : InvalidKey;
            Change(nextKey, immediate);
            return true;
        }

        /// <summary>
        /// スタックのクリア(現在のKeyは残る）
        /// </summary>
        public void ClearStack() {
            // 現在のKeyより前を削除
            while (_stack.Count > 1) {
                _stack.RemoveAt(0);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update(float deltaTime) {
            var state = default(TState);
            var currentKey = CurrentKey;
            
            // 遷移
            if (!NextKey.Equals(currentKey)) {
                // スタックに含まれていた物だった場合はそこまでのスタックをクリア
                var backIndex = _stack.IndexOf(NextKey);
                var back = backIndex >= 0;
                
                // 現在のステートを終了
                if (_states.TryGetValue(currentKey, out state)) {
                    state.OnExit(NextKey, back);
                    _scope.Clear();
                }
                
                // スタックをクリア
                if (back) {
                    for (var i = _stack.Count - 1; i >= backIndex; i--) {
                        _stack.RemoveAt(i);
                    }
                }

                // 次のステートを開始
                var prevKey = currentKey;
                currentKey = NextKey;
                if (_states.TryGetValue(currentKey, out state)) {
                    _stack.Add(currentKey);
                    state.OnEnter(prevKey, back, _scope);
                }
                else {
                    // 遷移先がInvalidKeyの場合、Stackを全部削除
                    _stack.Clear();
                }

                OnChangedState?.Invoke(prevKey, currentKey);
            }

            // 更新
            if (_states.TryGetValue(currentKey, out state)) {
                state.OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Stateの検索
        /// </summary>
        public TState FindState(TKey key) {
            if (_states.TryGetValue(key, out var state)) {
                return state;
            }

            return default;
        }
    }

    /// <summary>
    /// TState省略するタイプのStateContainer
    /// </summary>
    public class StateContainer<TKey> : StateContainer<IState<TKey>, TKey>
        where TKey : IComparable {
    }
}