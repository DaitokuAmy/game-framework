using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// アクター基底
    /// </summary>
    public abstract class Actor : IUpdatable, ILateUpdatable, IUpdatableEventHandler, ILateUpdatableEventHandler, IDisposable {
        private readonly Dictionary<Type, ActorComponent> _actorComponentDict = new(32);
        private readonly List<ActorComponent> _orderedActorComponents = new(32);
        private readonly GizmoDispatcher _gizmoDispatcher;

        private UpdateScheduler _updateScheduler;
        private UpdateScheduler _lateUpdateScheduler;
        private bool _disposed;
        private DisposableScope _activeScope;

        /// <inheritdoc/>
        bool IUpdatableBase.IsActive => IsActive;

        /// <summary>アクティブ状態</summary>
        public virtual bool IsActive => _activeScope != null;

        /// <summary>制御するBody</summary>
        public Body Body { get; }
        /// <summary>Transform情報</summary>
        public Transform Transform => Body.Transform;
        /// <summary>グローバル位置</summary>
        public Vector3 Position {
            get => Body.Position;
            set => Body.Position = value;
        }
        /// <summary>グローバル向き</summary>
        public Quaternion Rotation {
            get => Body.Rotation;
            set => Body.Rotation = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Actor(Body body) {
            Body = body;
            _gizmoDispatcher = body.GetComponent<GizmoDispatcher>();

            if (_gizmoDispatcher != null) {
                _gizmoDispatcher.DrawGizmosEvent += OnDrawGizmos;
                _gizmoDispatcher.DrawGizmosSelectedEvent += OnDrawGizmosSelected;
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            Deactivate();
            DisposeInternal();
            DisposeComponents();

            if (_updateScheduler != null) {
                _updateScheduler.UnregisterUpdatable(this);
                _updateScheduler = null;
            }

            if (_lateUpdateScheduler != null) {
                _lateUpdateScheduler.UnregisterLateUpdatable(this);
                _lateUpdateScheduler = null;
            }

            if (_gizmoDispatcher != null) {
                _gizmoDispatcher.DrawGizmosEvent -= OnDrawGizmos;
                _gizmoDispatcher.DrawGizmosSelectedEvent -= OnDrawGizmosSelected;
            }
        }

        /// <inheritdoc/>
        void IUpdatable.Update() {
            if (_activeScope != null) {
                var deltaTime = Body.LayeredTime.DeltaTime;
                for (var i = 0; i < _orderedActorComponents.Count; i++) {
                    var component = _orderedActorComponents[i];
                    component.Update(deltaTime);
                }

                UpdateInternal();
            }
        }

        /// <inheritdoc/>
        void ILateUpdatable.Update() {
            if (_activeScope != null) {
                var deltaTime = Body.LayeredTime.DeltaTime;
                for (var i = 0; i < _orderedActorComponents.Count; i++) {
                    var component = _orderedActorComponents[i];
                    component.LateUpdate(deltaTime);
                }

                LateUpdateInternal();
            }
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IUpdatable>.OnRegistered(UpdateScheduler updateScheduler) {
            _updateScheduler = updateScheduler;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<IUpdatable>.OnUnregistered(UpdateScheduler updateScheduler) {
            if (updateScheduler == _updateScheduler) {
                _updateScheduler = null;
            }
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnRegistered(UpdateScheduler updateScheduler) {
            _lateUpdateScheduler = updateScheduler;
        }

        /// <inheritdoc/>
        void IUpdatableEventHandlerBase<ILateUpdatable>.OnUnregistered(UpdateScheduler updateScheduler) {
            if (updateScheduler == _lateUpdateScheduler) {
                _lateUpdateScheduler = null;
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected virtual void ActivateInternal(IScope scope) {
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }

        /// <summary>
        /// 選択中ギズモ描画
        /// </summary>
        protected virtual void DrawGizmosSelectedInternal() {
        }

        /// <summary>
        /// ギズモ描画
        /// </summary>
        protected virtual void DrawGizmosInternal() {
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        public void Activate() {
            if (_disposed || _activeScope != null) {
                return;
            }

            _activeScope = new DisposableScope();
            ActivateInternal(_activeScope);
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        public void Deactivate() {
            if (_activeScope == null) {
                return;
            }

            _activeScope.Dispose();
            _activeScope = null;
        }

        /// <summary>
        /// Componentの取得
        /// </summary>
        public TComponent GetComponent<TComponent>()
            where TComponent : ActorComponent {
            if (!_actorComponentDict.TryGetValue(typeof(TComponent), out var component)) {
                return null;
            }

            return (TComponent)component;
        }

        /// <summary>
        /// Componentの追加
        /// </summary>
        public TComponent AddComponent<TComponent>(TComponent component)
            where TComponent : ActorComponent {
            var type = typeof(TComponent);
            if (!_actorComponentDict.TryAdd(type, component)) {
                throw new InvalidOperationException($"Actor component {type} has already been added.");
            }

            component.Initialize();
            _orderedActorComponents.Add(component);
            _orderedActorComponents.Sort((a, b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
            return component;
        }

        /// <summary>
        /// Componentの廃棄
        /// </summary>
        private void DisposeComponents() {
            foreach (var component in _orderedActorComponents) {
                component.Dispose();
            }

            _orderedActorComponents.Clear();
        }

        /// <summary>
        /// ギズモ描画通知(選択中)
        /// </summary>
        private void OnDrawGizmosSelected() {
            DrawGizmosSelectedInternal();

            foreach (var component in _orderedActorComponents) {
                component.DrawGizmosSelected();
            }
        }

        /// <summary>
        /// ギズモ描画通知
        /// </summary>
        private void OnDrawGizmos() {
            DrawGizmosInternal();

            foreach (var component in _orderedActorComponents) {
                component.DrawGizmos();
            }
        }
    }
}