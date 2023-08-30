using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.StateSystems {
    /// <summary>
    /// 列挙型ベースの状態制御クラス
    /// </summary>
    public class EnumStateContainer<TKey> : StateContainer<EnumStateContainer<TKey>.EnumState, TKey>
        where TKey : Enum {
        
        /// <summary>
        /// 開始アクション
        /// </summary>
        /// <param name="prevKey">ひとつ前の遷移Key</param>
        /// <param name="back">戻り遷移か</param>
        /// <param name="scope">Exitするまでのスコープ</param>
        public delegate void EnterAction(TKey prevKey, bool back, IScope scope);
        
        /// <summary>
        /// 更新アクション
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public delegate void UpdateAction(float deltaTime);
        
        /// <summary>
        /// 終了アクション
        /// </summary>
        /// <param name="nextKey">次の遷移Key</param>
        /// <param name="back">戻り遷移か</param>
        public delegate void ExitAction(TKey nextKey, bool back);
        
        /// <summary>
        /// Enum型をキーとしたState
        /// </summary>
        public class EnumState : IState<TKey> {
            public EnterAction OnEnterEvent;
            public UpdateAction OnUpdateEvent;
            public ExitAction OnExitEvent;

            public TKey Key { get; }

            public EnumState(TKey enumValue) {
                Key = enumValue;
            }

            void IState<TKey>.OnEnter(TKey prevKey, bool back, IScope scope) {
                OnEnterEvent?.Invoke(prevKey, back, scope);
            }

            void IState<TKey>.OnUpdate(float deltaTime) {
                OnUpdateEvent?.Invoke(deltaTime);
            }

            void IState<TKey>.OnExit(TKey nextKey, bool back) {
                OnExitEvent?.Invoke(nextKey, back);
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
        public void SetFunction(TKey key, EnterAction onEnter, UpdateAction onUpdate = null,
            ExitAction onExit = null) {
            var state = FindState(key);
            if (state == null) {
                Debug.LogError($"Invalid state. [{key}]");
                return;
            }

            state.OnEnterEvent = onEnter;
            state.OnUpdateEvent = onUpdate;
            state.OnExitEvent = onExit;
        }
    }
}