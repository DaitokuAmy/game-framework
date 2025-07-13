using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Actorインスタンス管理用のコア
    /// </summary>
    public class ActorEntity : IDisposable {
        private readonly List<IActorEntityComponent> _components = new();
        private readonly Dictionary<Type, IActorEntityComponent> _componentDict = new();

        /// <summary>識別子</summary>
        public int Id { get; }
        /// <summary>Active状態</summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorEntity(int id, bool active = true) {
            Id = id;
            IsActive = active;
        }

        /// <summary>
        /// アクティブ状態の変更
        /// </summary>
        /// <param name="active">Activeか</param>
        public void SetActive(bool active) {
            if (active == IsActive) {
                return;
            }

            IsActive = active;

            // ComponentのActive状態変更
            foreach (var component in _components) {
                if (active) {
                    component.Activate();
                }
                else {
                    component.Deactivate();
                }
            }
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        /// <param name="type">取得する型</param>
        public ActorEntityComponent GetComponent(Type type) {
            // 型一致での検索
            if (!_componentDict.TryGetValue(type, out var component)) {
                return null;
            }

            return (ActorEntityComponent)component;
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        public T GetComponent<T>()
            where T : ActorEntityComponent {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// Componentの追加(重複はエラー)
        /// </summary>
        /// <param name="type">追加する型</param>
        public ActorEntityComponent AddComponent(Type type) {
            if (!type.IsSubclassOf(typeof(ActorEntityComponent))) {
                Debug.LogError($"Component is not EntityComponent. [{type.Name}]");
                return null;
            }

            if (_componentDict.ContainsKey(type)) {
                Debug.LogError($"Already exists entity component. [{type.Name}]");
                return null;
            }

            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null) {
                Debug.LogError($"Not found default constructor. [{type.Name}]");
                return null;
            }

            var component = (IActorEntityComponent)constructor.Invoke(Array.Empty<object>());
            _components.Add(component);
            _componentDict[type] = component;
            component.Attached(this);
            if (IsActive) {
                component.Activate();
            }

            return (ActorEntityComponent)component;
        }

        /// <summary>
        /// Componentの追加(重複はエラー)
        /// </summary>
        public T AddComponent<T>()
            where T : ActorEntityComponent {
            return (T)AddComponent(typeof(T));
        }

        /// <summary>
        /// Componentの追加、既に追加されていたら取得
        /// </summary>
        /// <param name="type">追加する型</param>
        public ActorEntityComponent AddOrGetComponent(Type type) {
            var component = GetComponent(type);
            if (component == null) {
                component = AddComponent(type);
            }

            return component;
        }

        /// <summary>
        /// Componentの追加、既に追加されていたら取得
        /// </summary>
        public T AddOrGetComponent<T>()
            where T : ActorEntityComponent {
            return (T)AddOrGetComponent(typeof(T));
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            for (var i = _components.Count - 1; i >= 0; i--) {
                var component = _components[i];
                if (IsActive) {
                    component.Deactivate();
                }

                component.Detached(this);
                component.Dispose();
            }

            _components.Clear();
            _componentDict.Clear();
        }
    }
}