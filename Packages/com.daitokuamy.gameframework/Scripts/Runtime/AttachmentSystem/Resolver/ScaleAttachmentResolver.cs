using System;
using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// 拡大縮小追従用のResolver
    /// </summary>
    public class ScaleAttachmentResolver : AttachmentResolver {
        // 設定
        [Serializable]
        public class ResolverSettings {
            [Tooltip("スケールオフセット")]
            public Vector3 offsetScale = Vector3.one;
        }

        /// <summary>設定</summary>
        public ResolverSettings Settings { get; set; } = new ResolverSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象のTransform</param>
        public ScaleAttachmentResolver(Transform owner)
            : base(owner) {
        }

        /// <inheritdoc/>
        public override void Resolve() {
            Owner.localScale = Vector3.Scale(GetTargetLocalScale(), Settings.offsetScale);
        }

        /// <inheritdoc/>
        public override void ResetOffset() {
            Settings.offsetScale = Vector3.one;
        }

        /// <inheritdoc/>
        public override void TransferOffset() {
            // Scale
            var targetScale = GetTargetLocalScale();
            var localScale = Owner.localScale;
            Settings.offsetScale = new Vector3
            (
                localScale.x / targetScale.x,
                localScale.y / targetScale.y,
                localScale.z / targetScale.z
            );
        }
    }
}
