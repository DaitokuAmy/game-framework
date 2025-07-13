using GameFramework.ActorSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// アクター基底
    /// </summary>
    public abstract class MovableActor : ActionableActor, IMovableActor {
        /// <summary>地上にいるか</summary>
        public virtual bool IsGrounded => Body.Position.y <= float.Epsilon;
        /// <summary>地上の高さ</summary>
        public virtual float GroundHeight => 0.0f;
        
        /// <summary>移動制御用クラス</summary>
        protected MoveActorComponent Component { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected MovableActor(Body body) : base(body) {
            Component = new MoveActorComponent(this);
        }

        /// <summary>
        /// 現在座標の取得
        /// </summary>
        Vector3 IMovableActor.GetPosition() {
            return Body.Position;
        }

        /// <summary>
        /// 現在座標の更新
        /// </summary>
        void IMovableActor.SetPosition(Vector3 position) {
            SetPosition(position);
        }

        /// <summary>
        /// 現在向きの取得
        /// </summary>
        Quaternion IMovableActor.GetRotation() {
            return Body.Rotation;
        }

        /// <summary>
        /// 現在向きの更新
        /// </summary>
        void IMovableActor.SetRotation(Quaternion rotation) {
            SetRotation(rotation);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            Component.Dispose();
            
            base.DisposeInternal();
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            Component.Cancel();
            
            base.DeactivateInternal();
        }

        /// <summary>
        /// 現在座標の更新
        /// </summary>
        protected virtual void SetPosition(Vector3 position) {
            Body.Position = position;
        }

        /// <summary>
        /// 現在向きの更新
        /// </summary>
        protected virtual void SetRotation(Quaternion rotation) {
            Body.Rotation = rotation;
        }
    }
}