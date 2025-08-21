using GameFramework.ActorSystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// 注視向き制御用ActorComponent
    /// </summary>
    public sealed class LookAtControlActorComponent : ActorComponent {
        private readonly Actor _actor;
        private readonly Transform _lookDir;

        /// <summary>注視向き</summary>
        public Quaternion LookAtRotation { get; private set; } = Quaternion.identity;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="actor">制御対象のActor</param>
        public LookAtControlActorComponent(Actor actor) {
            _actor = actor;
            _lookDir = _actor.Body.Locators["LookDir"];
            if (_lookDir == _actor.Body.Transform) {
                _lookDir = null;
            }
        }

        /// <inheritdoc/>
        protected override void LateUpdateInternal(float deltaTime) {
            // LookDirに注視向きを設定
            if (_lookDir != null) {
                _lookDir.rotation = LookAtRotation;
            }
        }

        /// <inheritdoc/>
        protected override void DrawGizmosInternal() {
            var prevColor = Gizmos.color;
            Gizmos.color = Color.yellow;
            var from = _actor.Body.Transform.position + Vector3.up * 0.5f;
            var to = from + LookAtRotation * Vector3.forward;
            Gizmos.DrawLine(from, to);
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// 回転
        /// </summary>
        public void Rotate(float angleY) {
            LookAtRotation *= Quaternion.Euler(0, angleY, 0);
        }
    }
}