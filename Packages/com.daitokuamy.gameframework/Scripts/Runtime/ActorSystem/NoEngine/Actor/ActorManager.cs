using System;
using System.Collections.Generic;
using GameFramework.Pooling;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター管理用クラス
    /// </summary>
    public sealed class ActorManager<TKey> : IDisposable {
        private readonly IActorScheduler<TKey> _scheduler;
        private readonly List<IActorRuntime<TKey>> _actorRuntimes = new();
        private readonly Dictionary<TKey, Actor<TKey>> _actorMap = new();
        private readonly ObjectPool<Actor<TKey>> _actorPool;

        private bool _disposed;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorManager(IActorScheduler<TKey> scheduler, int poolMaxSize = 10000) {
            _actorPool = new ObjectPool<Actor<TKey>>(() => {
                var actor = new Actor<TKey>();
                return actor;
            }, actionOnDestroy: actor => {
                var runtime = (IActorRuntime<TKey>)actor;
                runtime.Dispose();
            }, maxSize: poolMaxSize);
            
            _scheduler = scheduler;
            _scheduler.RegisterManager(this);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            
            _scheduler.UnregisterManager(this);
            _actorMap.Clear();
            foreach (var actorRuntime in _actorRuntimes) {
                _actorPool.Release((Actor<TKey>)actorRuntime);
            }

            _actorPool.Dispose();

            _actorRuntimes.Clear();
        }

        /// <summary>
        /// アクターの生成
        /// </summary>
        /// <param name="id">識別子</param>
        public Actor<TKey> CreateActor(TKey id) {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(ActorManager<TKey>));
            }

            if (_actorMap.ContainsKey(id)) {
                throw new ArgumentException($"Actor already exists. key={id}");
            }

            var actor = _actorPool.Get();
            var actorRuntime = (IActorRuntime<TKey>)actor;
            actorRuntime.Initialize(id);
            _actorMap.Add(id, actor);
            _actorRuntimes.Add(actor);
            return actor;
        }

        /// <summary>
        /// アクターの削除
        /// </summary>
        /// <param name="id">識別子</param>
        public void DeleteActor(TKey id) {
            if (!_actorMap.Remove(id, out var actor)) {
                return;
            }

            var actorRuntime = (IActorRuntime<TKey>)actor;
            _actorRuntimes.Remove(actor);
            actorRuntime.Terminate();
            _actorPool.Release(actor);
        }

        /// <summary>
        /// アクターの削除
        /// </summary>
        /// <param name="actor">対象アクター</param>
        public void DeleteActor(Actor<TKey> actor) {
            if (actor == null) {
                return;
            }

            DeleteActor(actor.Id);
        }

        /// <summary>
        /// アクターの取得
        /// </summary>
        /// <param name="id">識別子</param>
        /// <param name="actor">見つかったアクター</param>
        /// <returns>あればtrue, なければfalse</returns>
        public bool TryGetActor(TKey id, out Actor<TKey> actor) {
            return _actorMap.TryGetValue(id, out actor);
        }

        /// <summary>
        /// アクターの存在チェック
        /// </summary>
        /// <param name="id">識別子</param>
        /// <returns>あればtrue, なければfalse</returns>
        public bool ContainsActor(TKey id) {
            return _actorMap.ContainsKey(id);
        }

        /// <summary>
        /// Controllerの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdateController(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var actorRuntime in _actorRuntimes) {
                actorRuntime.UpdateController(deltaTime);
            }
        }

        /// <summary>
        /// StateMachineの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdateStateMachine(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var actorRuntime in _actorRuntimes) {
                actorRuntime.UpdateStateMachine(deltaTime);
            }
        }

        /// <summary>
        /// Modelの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdateModel(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var actorRuntime in _actorRuntimes) {
                actorRuntime.UpdateModel(deltaTime);
            }
        }

        /// <summary>
        /// Presenterの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdatePresenter(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var actorRuntime in _actorRuntimes) {
                actorRuntime.UpdatePresenter(deltaTime);
            }
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdateView(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var actorRuntime in _actorRuntimes) {
                actorRuntime.UpdateView(deltaTime);
            }
        }

        /// <summary>
        /// Receiverの更新
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdateReceiver(float deltaTime) {
            if (_disposed) {
                return;
            }

            foreach (var actorRuntime in _actorRuntimes) {
                actorRuntime.UpdateReceiver(deltaTime);
            }
        }
    }
}