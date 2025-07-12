using GameFramework.Core;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorEntity用ロジック処理
    /// </summary>
    public abstract class ActorEntityLogic : Logic {
        private DisposableScope _attachScope;

        /// <summary>持ち主のEntity</summary>
        public ActorEntity ActorEntity { get; private set; }

        /// <summary>
        /// Entityに追加された時の処理
        /// </summary>
        /// <param name="actorEntity">持ち主のEntity</param>
        public void Attach(ActorEntity actorEntity) {
            if (ActorEntity != null || actorEntity == null) {
                return;
            }

            ActorEntity = actorEntity;
            _attachScope = new DisposableScope();
            AttachInternal(_attachScope);
        }

        /// <summary>
        /// Entityから削除された時の処理
        /// </summary>
        /// <param name="actorEntity">持ち主のEntity</param>
        public void Detach(ActorEntity actorEntity) {
            if (ActorEntity == null || actorEntity == null) {
                return;
            }

            if (actorEntity == ActorEntity) {
                DetachInternal();
                _attachScope.Dispose();
                _attachScope = null;
                ActorEntity = null;
            }
        }

        /// <summary>
        /// Entityに追加された時の処理
        /// </summary>
        protected virtual void AttachInternal(IScope scope) {
        }

        /// <summary>
        /// Entityから削除された時の処理
        /// </summary>
        /// <returns></returns>
        protected virtual void DetachInternal() {
        }
    }
}