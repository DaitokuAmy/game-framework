using System;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// 移動制御用コントローラ
    /// </summary>
    public class MoveController : IDisposable {
        private Transform _owner;
        private Vector3 _moveDirection;
        private Action<float> _updatePosition;

        // 角速度(度)
        public float AngularVelocity { get; set; }
        // 移動中
        public bool IsMoving { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveController(Transform owner, float angularVelocity, Action<float> updatePosition) {
            _owner = owner;
            AngularVelocity = angularVelocity;
            _updatePosition = updatePosition;
            IsMoving = false;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _owner = null;
            IsMoving = false;
        }

        /// <summary>
        /// 移動
        /// </summary>
        public void Move(Vector3 direction) {
            _moveDirection = direction;
            _moveDirection.y = 0.0f;
            _moveDirection.Normalize();
            IsMoving = _moveDirection.sqrMagnitude > 0.0001f;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            IsMoving = _moveDirection.sqrMagnitude > 0.0001f;

            if (!IsMoving) {
                _updatePosition?.Invoke(0.0f);
                return;
            }

            // 向きを揃える
            var angles = _owner.eulerAngles;
            var targetAngle = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg;
            angles.y = Mathf.MoveTowardsAngle(angles.y, targetAngle, AngularVelocity * deltaTime);
            _owner.eulerAngles = angles;

            // 該当方向に移動する
            var forward = _owner.forward;
            var forwardRate = Mathf.Max(0.0f, Vector3.Dot(forward, _moveDirection));
            _updatePosition?.Invoke(forwardRate);

            // 移動値をリセット
            _moveDirection = Vector3.zero;
        }
    }
}