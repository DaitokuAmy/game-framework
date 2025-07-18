using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// 状態制御クラス
    /// </summary>
    public class StateContainer<TState, TKey> : IDisposable
        where TState : IState<TKey>
        where TKey : IComparable {
        private readonly Dictionary<TKey, TState> _states = new();
        private readonly DisposableScope _scope = new();

        private bool _reset;

        /// <summary>状態変更通知(Prev > Next)</summary>
        public event Action<TKey, TKey> ChangedStateEvent;

        /// <summary>現在のステートキー</summary>
        public TKey CurrentKey { get; private set; }
        /// <summary>無効キー</summary>
        public TKey InvalidKey { get; private set; }
        /// <summary>遷移予定のステートキー</summary>
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
        /// <param name="invalidKey">無効なNullキー</param>
        /// <param name="useStack">スタック機能を使うか</param>
        /// <param name="states">設定するState情報</param>
        public void Setup(TKey invalidKey, bool useStack, params TState[] states) {
            Cleanup();

            InvalidKey = invalidKey;
            NextKey = invalidKey;
            CurrentKey = invalidKey;

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
        /// 初期化処理
        /// </summary>
        /// <param name="invalidKey">無効なNullキー</param>
        /// <param name="states">設定するState情報</param>
        public void Setup(TKey invalidKey, params TState[] states) {
            Setup(invalidKey, false, states);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Cleanup() {
            Change(InvalidKey, true);

            _states.Clear();
            CurrentKey = default;
            InvalidKey = default;
            NextKey = default;
        }

        /// <summary>
        /// Stateの変更
        /// </summary>
        /// <param name="key">Stateキー</param>
        /// <param name="immediate">即時変更するか</param>
        /// <param name="force">同じStateだとしても遷移を行うか</param>
        public void Change(TKey key, bool immediate = false, bool force = false) {
            if (key.Equals(NextKey)) {
                if (force) {
                    _reset = true;
                }
                else {
                    return;
                }
            }

            // 遷移先登録
            NextKey = key;

            // 即時反映する場合、更新を実行
            if (immediate) {
                Update(0.0f);
            }
        }

        /// <summary>
        /// Stateのリセット
        /// </summary>
        /// <param name="immediate">即時変更するか</param>
        public void Reset(bool immediate = false) {
            Change(CurrentKey, immediate, true);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update(float deltaTime) {
            var state = default(TState);
            var currentKey = CurrentKey;

            // 遷移
            if (!NextKey.Equals(currentKey) || _reset) {
                var reset = _reset;
                _reset = false;

                // 現在のステートを終了
                if (_states.TryGetValue(currentKey, out state)) {
                    state.OnExit(NextKey);
                    _scope.Clear();
                }

                // 次のステートを開始
                var prevKey = currentKey;
                currentKey = NextKey;
                if (_states.TryGetValue(currentKey, out state)) {
                    if (!reset) {
                        CurrentKey = currentKey;
                    }

                    state.OnEnter(prevKey, _scope);
                }
                else {
                    CurrentKey = InvalidKey;
                }

                ChangedStateEvent?.Invoke(prevKey, currentKey);
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