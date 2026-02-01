using GameFramework.ActorSystem;
using GameFramework;

namespace SampleGameEngine {
    /// <summary>
    /// 移動制御用ActorComponent
    /// </summary>
    public sealed class MoveActorComponent : ActorViewComponent {
        private readonly ActorMover _mover;

        private AsyncOperator _moveAsyncOperator;
        
        /// <inheritdoc/>
        public override int ExecutionOrder => (int)ActorComponentOrder.Move;
        
        /// <summary>現在移動中かどうか</summary>
        public bool IsMoving => _mover.IsMoving;
        /// <summary>移動制御クラス</summary>
        public ActorMover Mover => _mover;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveActorComponent() {
            _mover = new ActorMover();
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            _mover.Stop();
            base.DisposeInternal();
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(float deltaTime) {
            _mover.Tick(deltaTime);
        }
    }
}