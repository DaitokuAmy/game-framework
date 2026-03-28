using System;
using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクターステートマシン
    /// </summary>
    public sealed class ActorStateMachine<TKey, TBlackboard> : IActorStateMachine
        where TBlackboard : class, IActorStateBlackboard, new() {
        private readonly Dictionary<Type, ActorState<TKey, TBlackboard>> _stateMap = new();

        private bool _disposed;
        private Type _currentStateType;
        private IActorState _currentState;

        /// <summary>現在のステート</summary>
        public ActorState<TKey, TBlackboard> CurrentState => (ActorState<TKey, TBlackboard>)_currentState;
        /// <summary>所有主アクター</summary>
        public Actor<TKey> Owner { get; }
        /// <summary>ブラックボード</summary>
        public TBlackboard Blackboard { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">所有主</param>
        public ActorStateMachine(Actor<TKey> owner) {
            Owner = owner;
            Blackboard = new();
        }

        /// <inheritdoc/>
        void IDisposable.Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            ResetState();
            _stateMap.Clear();
        }

        /// <inheritdoc/>
        void IActorStateMachine.Update(IReadOnlyList<ActorCommand> commands, IReadOnlyList<ActorSignal> signals, float deltaTime) {
            if (_currentState != null) {
                var nextType = _currentState.Update(commands, signals, deltaTime);
                if (nextType != null) {
                    ChangeState(nextType);
                }
            }
        }

        /// <summary>
        /// ステートの変更
        /// </summary>
        /// <param name="type">変更するステートの型</param>
        private void ChangeState(Type type) {
            if (_disposed) {
                return;
            }

            // ステートチェック
            if (!_stateMap.TryGetValue(type, out var nextState)) {
                throw new ArgumentException($"State type is not found. type={type}");
            }

            // 終了処理
            if (_currentState != null) {
                var state = _currentState;
                _currentState = null;
                _currentStateType = null;
                state.Exit();
            }

            // 開始処理
            _currentState = nextState;
            _currentStateType = type;
            _currentState.Enter();
        }

        /// <summary>
        /// カレントステートが無い状態にリセットする
        /// </summary>
        public void ResetState() {
            // 終了処理
            if (_currentState != null) {
                var state = _currentState;
                _currentState = null;
                _currentStateType = null;
                state.Exit();
            }
        }

        /// <summary>
        /// ステートの取得
        /// </summary>
        public T GetState<T>()
            where T : class, IActorState {
            if (!_stateMap.TryGetValue(typeof(T), out var state)) {
                return null;
            }

            return state as T;
        }
        
        /// <summary>
        /// ステートリストの設定
        /// </summary>
        /// <param name="startStateType">開始ステートタイプ</param>
        /// <param name="states">ステートリスト</param>
        public void SetStates(Type startStateType, params ActorState<TKey, TBlackboard>[] states) {
            ResetState();
            
            _stateMap.Clear();
            foreach (var state in states) {
                var key = state.GetType();
                if (_stateMap.TryAdd(key, state)) {
                    state.Setup(this);
                }
            }
            
            ChangeState(startStateType);
        }
    }
}
