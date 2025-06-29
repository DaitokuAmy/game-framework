using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// LogicをEntityと紐づけるためのComponent
    /// </summary>
    [Preserve]
    public sealed class LogicComponent : Component {
        // ロジックのキャッシュ
        private Dictionary<Type, ActorEntityLogic> _logics = new Dictionary<Type, ActorEntityLogic>();

        /// <summary>
        /// ロジックの取得
        /// </summary>
        public TLogic GetLogic<TLogic>()
            where TLogic : ActorEntityLogic {
            var type = typeof(TLogic);
            if (_logics.TryGetValue(type, out var logic)) {
                return (TLogic)logic;
            }

            foreach (var pair in _logics) {
                if (type.IsAssignableFrom(pair.Key)) {
                    return (TLogic)pair.Value;
                }
            }

            return default;
        }

        /// <summary>
        /// ロジックが含まれているか
        /// </summary>
        public bool ContainsLogic(ActorEntityLogic logic) {
            return _logics.ContainsValue(logic);
        }

        /// <summary>
        /// ロジックの追加(Remove時に自動削除)
        /// </summary>
        /// <param name="logic">追加するLogic</param>
        public ActorEntity AddLogic(ActorEntityLogic logic) {
            var type = logic.GetType();
            if (_logics.ContainsKey(type)) {
                Debug.LogError($"Already exists logic. type:{type.Name}");
                return Entity;
            }

            if (logic.ActorEntity != null) {
                Debug.LogError($"Entity is not null. type:{type.Name}");
                return Entity;
            }

            _logics[type] = logic;
            logic.Attach(Entity);
            if (Entity.IsActive) {
                logic.Activate();
            }

            return Entity;
        }

        /// <summary>
        /// ロジックの削除
        /// </summary>
        /// <param name="logic">削除対象のLogic</param>
        /// <param name="dispose">LogicをDisposeするか</param>
        public ActorEntity RemoveLogic(ActorEntityLogic logic, bool dispose = true) {
            var type = logic.GetType();
            if (!_logics.ContainsKey(type)) {
                return Entity;
            }

            _logics.Remove(type);
            logic.Deactivate();
            logic.Detach(Entity);
            if (dispose) {
                logic.Dispose();
            }

            return Entity;
        }

        /// <summary>
        /// ロジックの削除
        /// </summary>
        /// <param name="dispose">LogicをDisposeするか</param>
        public ActorEntity RemoveLogic<TLogic>(bool dispose = true)
            where TLogic : Logic {
            var type = typeof(TLogic);
            if (!_logics.TryGetValue(type, out var logic)) {
                return Entity;
            }

            return RemoveLogic(logic, dispose);
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            foreach (var logic in _logics.Values) {
                logic.Dispose();
            }

            _logics.Clear();
        }
    }
}