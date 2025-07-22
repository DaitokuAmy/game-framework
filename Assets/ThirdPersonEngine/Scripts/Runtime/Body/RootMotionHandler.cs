using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// RootMotion用のハンドラ
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class RootMotionHandler : MonoBehaviour {
        [SerializeField, Tooltip("ルート移動にかけるスケール")]
        private Vector3 _positionScale = Vector3.one;
        [SerializeField, Tooltip("ルート回転にかけるスケール")]
        private Vector3 _anglesScale = Vector3.one;

        private Animator _animator;
        private Rigidbody _rigidbody;
        private CharacterController _characterController;

        /// <summary>ルート移動にかけるスケール</summary>
        public Vector3 PositionScale {
            get => _positionScale;
            set => _positionScale = value;
        }
        /// <summary>ルート回転にかけるスケール</summary>
        public Vector3 AnglesScale {
            get => _anglesScale;
            set => _anglesScale = value;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _characterController = GetComponent<CharacterController>();
        }

        /// <summary>
        /// RootMotionの適用時イベント
        /// </summary>
        private void OnAnimatorMove() {
            if (_animator.applyRootMotion) {
                var localDeltaPosition = transform.InverseTransformVector(_animator.deltaPosition);
                var localDeltaAngles = _animator.deltaRotation.eulerAngles;

                localDeltaPosition = Vector3.Scale(localDeltaPosition, PositionScale);
                localDeltaAngles = Vector3.Scale(localDeltaAngles, AnglesScale);

                var deltaPosition = transform.TransformVector(localDeltaPosition);
                var position = transform.position + deltaPosition;
                var rotation = transform.rotation * Quaternion.Euler(localDeltaAngles);

                if (_rigidbody != null) {
                    _rigidbody.Move(position, rotation);
                }
                else if (_characterController != null) {
                    _characterController.Move(deltaPosition);
                    transform.rotation = rotation;
                }
                else {
                    transform.position = position;
                    transform.rotation = rotation;
                }
            }
        }
    }
}