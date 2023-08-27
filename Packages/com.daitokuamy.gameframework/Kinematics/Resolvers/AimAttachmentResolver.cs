using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// エイム追従用のResolver
    /// </summary>
    public class AimAttachmentResolver : AttachmentResolver {
        // 設定
        [Serializable]
        public class ResolverSettings {
            [Tooltip("制御空間")]
            public Space space = Space.Self;
            [Tooltip("角度オフセット")]
            public Vector3 offsetAngles = Vector3.zero;
            [Tooltip("正面のベクトル")]
            public Vector3 forwardVector = Vector3.forward;
            [Tooltip("上のベクトル")]
            public Vector3 upVector = Vector3.up;
            [Tooltip("UpベクトルをさすTransform(未指定はデフォルト)")]
            public Transform worldUpObject = null;
        }

        // 設定
        public ResolverSettings Settings { get; set; } = new ResolverSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象のTransform</param>
        public AimAttachmentResolver(Transform owner)
            : base(owner) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void Resolve() {
            var space = Settings.space;
            var offsetRotation = Quaternion.Euler(Settings.offsetAngles);
            var axisRotation = Quaternion.Inverse(Quaternion.LookRotation(Settings.forwardVector, Settings.upVector));
            var upVector = Settings.worldUpObject != null ? Settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - Owner.position, upVector) * axisRotation;

            if (space == Space.Self) {
                Owner.rotation = baseRotation * offsetRotation;
            }
            else {
                Owner.rotation = offsetRotation * baseRotation;
            }
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            Settings.offsetAngles = Vector3.zero;
        }

        /// <summary>
        /// 自身のTransformからオフセットを設定する
        /// </summary>
        public override void TransferOffset() {
            var space = Settings.space;
            var axisRotation = Quaternion.Inverse(Quaternion.LookRotation(Settings.forwardVector, Settings.upVector));
            var upVector = Settings.worldUpObject != null ? Settings.worldUpObject.up : Vector3.up;
            var baseRotation =
                Quaternion.LookRotation(GetTargetPosition() - Owner.position, upVector) * axisRotation;

            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(baseRotation) * Owner.rotation;
            }
            else {
                offsetRotation = Owner.rotation * Quaternion.Inverse(baseRotation);
            }

            Settings.offsetAngles = offsetRotation.eulerAngles;
        }
    }
}