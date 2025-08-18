using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// FSMクラス
    /// </summary>
    public class FiniteStateMachine<TState, TKey> : IDisposable
        where TState : IState<TKey>
        where TKey : IComparable {
        private readonly Dictionary<TKey, TState> _states = new();
        private readonly DisposableScope _scope = new();

        /// <summary>状態変更通知(Prev > Next)</summary>
        public event Action<TKey, TKey> ChangedPrevNextStateEvent;
        /// <summary>状態変更通知(Next)</summary>
        public event Action<TKey> ChangedStateEvent;

        /// <summary>現在のステートキー</summary>
        public TKey CurrentKey { get; private set; }
        /// <summary>無効キー</summary>
        public TKey InvalidKey { get; private set; }

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
        /// <param name="states">設定するState情報</param>
        public void Setup(TKey invalidKey, params TState[] states) {
            Cleanup();

            InvalidKey = invalidKey;
            CurrentKey = invalidKey;

            foreach (var state in states) {
                // 無効キーは登録しない
                if (state.Key.Equals(invalidKey)) {
                    continue;
                }

                if (!_states.TryAdd(state.Key, state)) {
                    throw new Exception($"Already exists state key. {state.Key}");
                }
            }
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Cleanup() {
            Change(InvalidKey, true);

            _states.Clear();
            CurrentKey = default;
            InvalidKey = default;
        }

        /// <summary>
        /// Stateの変更
        /// </summary>
        /// <param name="key">Stateキー</param>
        /// <param name="force">同じStateだとしても遷移を行うか</param>
        public void Change(TKey key, bool force = false) {
            if (key.Equals(CurrentKey) && !force) {
                return;
            }

            var prevKey = CurrentKey;
            var nextKey = key;

            // 現在のステートを終了
            if (_states.TryGetValue(prevKey, out var state)) {
                state.OnExit(nextKey);
                _scope.Clear();
            }

            // 次のステートを開始
            if (_states.TryGetValue(nextKey, out state)) {
                CurrentKey = nextKey;
                state.OnEnter(prevKey, _scope);
            }
            else {
                CurrentKey = InvalidKey;
            }

            ChangedPrevNextStateEvent?.Invoke(prevKey, CurrentKey);
            ChangedStateEvent?.Invoke(CurrentKey);
        }

        /// <summary>
        /// Stateのリセット
        /// </summary>
        public void Reset() {
            Change(CurrentKey, true);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void Update(float deltaTime) {
            // 更新
            if (_states.TryGetValue(CurrentKey, out var state)) {
                state.OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Stateの検索
        /// </summary>
        public TState FindState(TKey key) {
            return _states.GetValueOrDefault(key);
        }
    }

    /// <summary>
    /// TState省略するタイプのFiniteStateMachine
    /// </summary>
    public class FiniteStateMachine<TKey> : FiniteStateMachine<IState<TKey>, TKey>
        where TKey : IComparable {
    }
}