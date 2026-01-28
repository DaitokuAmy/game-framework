#if USE_ANIMATION_RIGGING
using System;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// TwoBoneIK制御用パーツ
    /// </summary>
    public class TwoBoneIKRigParts : RigParts {
        /// <summary>
        /// ターゲット情報
        /// </summary>
        [Serializable]
        private class TargetInfo {
            [Tooltip("ターゲットオブジェクト")]
            public GameObject target;
            [Tooltip("座標設定時のオフセット座標")]
            public Vector3 offsetPosition;
            [Tooltip("座標設定時のオフセット角度")]
            public Vector3 offsetAngles;
        }

        [SerializeField, Tooltip("ターゲット情報")]
        private TargetInfo[] _targetInfos;

        /// <summary>
        /// ターゲット座標の設定
        /// </summary>
        /// <param name="position">座標</param>
        /// <param name="index">ターゲットIndex</param>
        public void SetTargetPosition(Vector3 position, int index = 0) {
            if (index < 0 || index >= _targetInfos.Length) {
                return;
            }

            var targetInfo = _targetInfos[index];
            if (targetInfo.target == null) {
                return;
            }

            var localPos = transform.InverseTransformPoint(position);
            targetInfo.target.transform.localPosition = localPos + targetInfo.offsetPosition;
        }

        /// <summary>
        /// ターゲット向きの設定
        /// </summary>
        /// <param name="rotation">向き</param>
        /// <param name="index">ターゲットIndex</param>
        public void SetTargetRotation(Quaternion rotation, int index = 0) {
            if (index < 0 || index >= _targetInfos.Length) {
                return;
            }

            var targetInfo = _targetInfos[index];
            if (targetInfo.target == null) {
                return;
            }

            var localRotation = rotation * Quaternion.Inverse(transform.rotation);
            targetInfo.target.transform.localRotation = localRotation * Quaternion.Euler(targetInfo.offsetAngles);
        }
    }
}
#endif