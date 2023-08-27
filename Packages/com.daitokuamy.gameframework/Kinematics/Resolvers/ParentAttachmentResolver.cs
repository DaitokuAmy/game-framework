using System;
using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Transform追従用Resolver
    /// </summary>
    public class ParentAttachmentResolver : AttachmentResolver {
        // 設定
        [Serializable]
        public class ResolverSettings {
            public Space space = Space.Self;
            public Vector3 offsetPosition = Vector3.zero;
            public Vector3 offsetAngles = Vector3.zero;
            public Vector3 offsetScale = Vector3.one;
            public TransformMasks mask = KinematicsDefinitions.TransformMasksAll;
        }

        // 設定
        public ResolverSettings Settings { get; set; } = new ResolverSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="owner">制御対象のTransform</param>
        public ParentAttachmentResolver(Transform owner)
            : base(owner) {
        }

        /// <summary>
        /// Transformを反映
        /// </summary>
        public override void Resolve() {
            var space = Settings.space;
            var offsetPosition = Settings.offsetPosition;
            var rotation = Quaternion.Euler(Settings.offsetAngles);
            var offsetScale = Settings.offsetScale;

            if (space == Space.Self) {
                offsetPosition = Owner.TransformVector(offsetPosition);
                rotation = GetTargetRotation() * rotation;
            }
            else {
                rotation = rotation * GetTargetRotation();
            }

            if ((Settings.mask & TransformMasks.Position) != 0) {
                Owner.position = GetTargetPosition() + offsetPosition;
            }

            if ((Settings.mask & TransformMasks.Rotation) != 0) {
                Owner.rotation = rotation;
            }

            if ((Settings.mask & TransformMasks.Scale) != 0) {
                Owner.localScale = Vector3.Scale(GetTargetLocalScale(), offsetScale);
            }
        }

        /// <summary>
        /// オフセットを初期化
        /// </summary>
        public override void ResetOffset() {
            Settings.offsetPosition = Vector3.zero;
            Settings.offsetAngles = Vector3.zero;
            Settings.offsetScale = Vector3.one;
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

            // Rotation
            Quaternion offsetRotation;

            if (space == Space.Self) {
                offsetRotation = Quaternion.Inverse(GetTargetRotation()) * Owner.rotation;
            }
            else {
                offsetRotation = Owner.rotation * Quaternion.Inverse(GetTargetRotation());
            }

            Settings.offsetAngles = offsetRotation.eulerAngles;

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