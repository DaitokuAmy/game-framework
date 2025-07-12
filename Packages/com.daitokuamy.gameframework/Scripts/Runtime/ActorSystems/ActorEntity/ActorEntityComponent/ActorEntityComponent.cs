using System;
using System.Threading;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Entity拡張用コンポーネント基底
    /// </summary>
    [Preserve]
    public abstract class ActorEntityComponent : IActorEntityComponent, IScope {
        // Attach中のScope
        private DisposableScope _attachScope = new DisposableScope();
        // Active中のScope
        private DisposableScope _activeScope = new DisposableScope();
        // キャンセル用
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>AttachされているEntity</summary>
        public ActorEntity Entity { get; private set; } = null;
        /// <summary>キャンセル用トークン</summary>
        public CancellationToken Token => _cancellationTokenSource.Token;

        /// <summary>Scope終了通知</summary>
        public event Action ExpiredEvent;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            DisposeInternal();
            _activeScope.Dispose();
            _activeScope = null;
            _attachScope.Dispose();
            _attachScope = null;
            ExpiredEvent?.InvokeDescending();
            ExpiredEvent = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Entityに登録された時の処理
        /// </summary>
        /// <param name="actorEntity">対象のEntity</param>
        void IActorEntityComponent.Attached(ActorEntity actorEntity) {
            if (Entity != null) {
                Debug.LogError($"Already attached component. {GetType().Name}");
                return;
            }

            Entity = actorEntity;
            AttachedInternal(_attachScope);
        }

        /// <summary>
        /// アクティブ化
        /// </summary>
        void IActorEntityComponent.Activate() {
            ActivateInternal(_activeScope);
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        void IActorEntityComponent.Deactivate() {
            DeactivateInternal();
            _activeScope.Clear();
        }

        /// <summary>
        /// Entityから登録解除された時の処理
        /// </summary>
        /// <param name="actorEntity">対象のEntity</param>
        void IActorEntityComponent.Detached(ActorEntity actorEntity) {
            if (actorEntity != Entity) {
                Debug.LogError($"Invalid detached entity. {GetType().Name}");
                return;
            }

            DetachedInternal();
            _attachScope.Clear();
            Entity = null;
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// Entityに登録された時の処理
        /// </summary>
        protected virtual void AttachedInternal(IScope scope) {
        }

        /// <summary>
        /// アクティブ化時の処理
        /// </summary>
        protected virtual void ActivateInternal(IScope scope) {
        }

        /// <summary>
        /// 非アクティブ化時の処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }

        /// <summary>
        /// Entityから登録解除された時の処理
        /// </summary>
        protected virtual void DetachedInternal() {
        }
    }
}