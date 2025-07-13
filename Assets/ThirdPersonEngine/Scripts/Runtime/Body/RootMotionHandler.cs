using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// RootMotion用のハンドラ
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class RootMotionHandler : MonoBehaviour {
        private Animator _animator;
        private Rigidbody _rigidbody;
        
        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// RootMotionの適用時イベント
        /// </summary>
        private void OnAnimatorMove() {
            if (_animator.applyRootMotion) {
                if (_rigidbody != null) {
                    _rigidbody.Move(_animator.rootPosition, _animator.rootRotation);
                }

                transform.position = _animator.rootPosition;
                transform.rotation = _animator.rootRotation;
            }
        }
    }
}