using System;
using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター更新管理用クラス
    /// </summary>
    public sealed class ActorScheduler<TKey> : IActorScheduler<TKey>, IDisposable {
        private readonly List<ActorManager<TKey>> _actorManagers = new();

        private bool _disposed;

        /// <inheritdoc/>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            _actorManagers.Clear();
        }

        /// <inheritdoc/>
        void IActorScheduler<TKey>.RegisterManager(ActorManager<TKey> manager) {
            _actorManagers.Add(manager);
        }

        /// <inheritdoc/>
        void IActorScheduler<TKey>.UnregisterManager(ActorManager<TKey> manager) {
            _actorManagers.Remove(manager);
        }

        /// <summary>
        /// 操作などの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void PreUpdate(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var manager in _actorManagers) {
                manager.UpdateController(deltaTime);
            }
        }

        /// <summary>
        /// ロジック更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdateLogic(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var manager in _actorManagers) {
                manager.UpdateStateMachine(deltaTime);
            }

            foreach (var manager in _actorManagers) {
                manager.UpdateModel(deltaTime);
            }
        }

        /// <summary>
        /// 見た目の更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdatePresentation(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var manager in _actorManagers) {
                manager.UpdatePresenter(deltaTime);
            }

            foreach (var manager in _actorManagers) {
                manager.UpdateView(deltaTime);
            }
        }

        /// <summary>
        /// 物理確定後などの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void PostUpdate(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var manager in _actorManagers) {
                manager.UpdateReceiver(deltaTime);
            }
        }
    }
}
