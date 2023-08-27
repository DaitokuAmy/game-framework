using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// 注視追従用のResolver
    /// </summary>
    public class LookAtAttachmentResolver : AttachmentResolver {
        // 追従設定
        [Serializable]
        public class ResolverSettings {
            [Tooltip("制御空間")]
            public Space space = Space.Self;
            [Tooltip("角度オフセット")]
            public Vector3 offsetAngles = Vector3.zero;
            [Tooltip("ねじり角度")]
            public float roll = 0.0f;
            [Tooltip("UpベクトルをさすTransform(未指定はデフォルト)")]
            public Transform worldUpObject = null;
        }

        // 設定
        public ResolverSettings Settings { get; set; } = new ResolverSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LookAtAttachmentResolver(Transform owner)
            : base(owner) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void Resolve() {
            var space = Settings.space;
            var offsetRotation = Quaternion.Euler(Settings.offsetAngles);
            var upVector = Settings.worldUpObject != null ? Settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - Owner.position, upVector) *
                Quaternion.Euler(0.0f, 0.0f, Settings.roll);

            if (space == Space.Self) {
                Owner.rotation = baseRotation * offsetRotation;
            }
            else {
                Owner.rotation = offsetRotation * baseRotation;
            }
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            var space = Settings.space;
            var upVector = Settings.worldUpObject != null ? Settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - Owner.position, upVector) *
                Quaternion.Euler(0.0f, 0.0f, Settings.roll);

            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(baseRotation) * Owner.rotation;
            }
            else {
                offsetRotation = Owner.rotation * Quaternion.Inverse(baseRotation);
            }

            Settings.offsetAngles = offsetRotation.eulerAngles;
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            Settings.offsetAngles = Vector3.zero;
        }
    }
}