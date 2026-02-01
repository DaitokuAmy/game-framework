using GameFramework.ActorSystem;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// 移動可能アクタービュー基底
    /// </summary>
    public abstract class MovableActorView : ActionableActorView, IMovableActor {
        private readonly MoveActorComponent _moveComponent;
        
        /// <inheritdoc/>
        public virtual bool IsGrounded => Body.Position.y <= float.Epsilon;
        /// <inheritdoc/>
        public virtual float GroundHeight => float.MinValue;

        /// <summary>移動制御用クラス</summary>
        protected ActorMover Mover => _moveComponent.Mover;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected MovableActorView(Body body) : base(body) {
            _moveComponent = new MoveActorComponent();
            AddComponent(_moveComponent);
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            _moveComponent.Mover.Stop();
            base.DisposeInternal();
        }

        /// <inheritdoc/>
        void IMovable.ApplyMove(Vector3 worldDelta, bool warp) {
            ApplyMove(worldDelta, warp);
        }

        /// <inheritdoc/>
        void IMovable.ApplyRotation(Quaternion rotation) {
            ApplyRotation(rotation);
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal() {
            _moveComponent.Mover.Stop();
            base.DeactivateInternal();
        }

        /// <summary>
        /// 現在座標の更新
        /// </summary>
        protected virtual void ApplyMove(Vector3 worldDelta, bool warp) {
            Body.Position += worldDelta;
        }

        /// <summary>
        /// 現在向きの更新
        /// </summary>
        protected virtual void ApplyRotation(Quaternion rotation) {
            Body.Rotation = rotation;
        }
    }
}