using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// 列挙型ベースの状態制御クラス
    /// </summary>
    public class EnumFiniteStateMachine<TKey> : FiniteStateMachine<EnumFiniteStateMachine<TKey>.EnumState, TKey>
        where TKey : Enum {
        
        /// <summary>
        /// 開始アクション
        /// </summary>
        /// <param name="prevKey">ひとつ前の遷移Key</param>
        /// <param name="scope">Exitするまでのスコープ</param>
        public delegate void EnterAction(TKey prevKey, IScope scope);
        
        /// <summary>
        /// 更新アクション
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public delegate void UpdateAction(float deltaTime);
        
        /// <summary>
        /// 終了アクション
        /// </summary>
        /// <param name="nextKey">次の遷移Key</param>
        public delegate void ExitAction(TKey nextKey);
        
        /// <summary>
        /// Enum型をキーとしたState
        /// </summary>
        public class EnumState : IState<TKey> {
            public EnterAction EnterEvent;
            public UpdateAction UpdateEvent;
            public ExitAction ExitEvent;

            public TKey Key { get; }

            public EnumState(TKey enumValue) {
                Key = enumValue;
            }

            void IState<TKey>.OnEnter(TKey prevKey, IScope scope) {
                EnterEvent?.Invoke(prevKey, scope);
            }

            void IState<TKey>.OnUpdate(float deltaTime) {
                UpdateEvent?.Invoke(deltaTime);
            }

            void IState<TKey>.OnExit(TKey nextKey) {
                ExitEvent?.Invoke(nextKey);
            }
        }

        /// <summary>
        /// 初期化処理(enum用)
        /// </summary>
        public void SetupFromEnum(TKey invalidKey) {
            var states = new List<EnumState>();
            foreach (TKey key in Enum.GetValues(typeof(TKey))) {
                if (key.Equals(invalidKey)) {
                    continue;
                }

                states.Add(new EnumState(key));
            }

            Setup(invalidKey, states.ToArray());
        }

        /// <summary>
        /// 関数登録
        /// </summary>
        /// <param name="key">登録対象のキー</param>
        /// <param name="onEnter">状態開始時処理</param>
        /// <param name="onUpdate">状態更新中処理</param>
        /// <param name="onExit">状態終了時処理</param>
        public void SetFunction(TKey key, EnterAction onEnter, UpdateAction onUpdate = null, ExitAction onExit = null) {
            var state = FindState(key);
            if (state == null) {
                throw new Exception($"Invalid state. [{key}]");
            }

            state.EnterEvent = onEnter;
            state.UpdateEvent = onUpdate;
            state.ExitEvent = onExit;
        }
    }
}