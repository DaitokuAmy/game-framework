using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 座標追従用のResolver
    /// </summary>
    public class PositionAttachmentResolver : AttachmentResolver {
        // 設定
        [Serializable]
        public class ResolverSettings {
            [Tooltip("制御空間")]
            public Space space = Space.Self;
            [Tooltip("座標オフセット")]
            public Vector3 offsetPosition;
        }

        // 設定
        public ResolverSettings Settings { get; set; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象のTransform</param>
        public PositionAttachmentResolver(Transform owner)
            : base(owner) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void Resolve() {
            var space = Settings.space;
            var offset = Settings.offsetPosition;

            if (space == Space.Self) {
                offset = Owner.TransformVector(offset);
            }

            Owner.position = GetTargetPosition() + offset;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            var space = Settings.space;
            // Position
            var offsetPosition = Owner.position - GetTargetPosition();

            if (space == Space.Self) {
                offsetPosition = Owner.InverseTransformVector(offsetPosition);
            }

            Settings.offsetPosition = offsetPosition;
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            Settings.offsetPosition = Vector3.zero;
        }
    }
}