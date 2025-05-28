using GameFramework.BodySystems;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// BodyをEntityと紐づけるためのComponent
    /// </summary>
    [Preserve]
    public sealed class BodyComponent : Component {
        // 最後に残っていたBodyのTransform情報
        private Vector3? _lastPosition;
        private Quaternion? _lastRotation;
        private float? _lastScale;

        // 現在のBody
        public Body Body { get; private set; } = null;

        /// <summary>
        /// Bodyの設定
        /// </summary>
        /// <param name="body">設定するBody</param>
        /// <param name="prevDispose">既に設定されているBodyをDisposeするか</param>
        public ActorEntity SetBody(Body body, bool prevDispose = true) {
            if (Body != null && Body.IsValid) {
                _lastPosition = Body.Position;
                _lastRotation = Body.Rotation;
                _lastScale = Body.BaseScale;

                if (prevDispose) {
                    Body?.Dispose();
                }
            }

            Body = body;

            if (Body != null) {
                Body.IsActive = Entity.IsActive;

                if (_lastPosition.HasValue) {
                    Body.Position = _lastPosition.Value;
                }

                if (_lastRotation.HasValue) {
                    Body.Rotation = _lastRotation.Value;
                }

                if (_lastScale.HasValue) {
                    Body.BaseScale = _lastScale.Value;
                }
            }

            return Entity;
        }

        /// <summary>
        /// Bodyの削除
        /// </summary>
        /// <param name="dispose">BodyをDisposeするか</param>
        public ActorEntity RemoveBody(bool dispose = true) {
            return SetBody(null, dispose);
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            if (Body != null) {
                Body.IsActive = true;
            }
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        protected override void DeactivateInternal() {
            if (Body != null) {
                Body.IsActive = false;
            }
        }

        /// <summary>
        /// 廃棄処理(override用)
        /// </summary>
        protected override void DisposeInternal() {
            RemoveBody();
        }
    }
}