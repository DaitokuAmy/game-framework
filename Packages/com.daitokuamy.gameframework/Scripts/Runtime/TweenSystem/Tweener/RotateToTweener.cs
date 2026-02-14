using UnityEngine;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// Transformのrotationをtoへ補間するTweener
    /// </summary>
    public sealed class RotateToTweener : Tweener {
        private Transform _target = null!;
        private Quaternion _from;
        private Quaternion _to;
        private Space _space;

        /// <summary>
        /// 初期化（fromはBegin時にキャプチャ）
        /// </summary>
        public RotateToTweener Setup(Transform target, Vector3 toEuler, float duration, Space space = Space.World) {
            _target = target;
            _to = Quaternion.Euler(toEuler);
            _space = space;
            SetupDuration(duration);
            return this;
        }

        /// <inheritdoc/>
        protected override void OnBegin() {
            if (_target == null) {
                _from = Quaternion.identity;
                return;
            }

            _from = _space == Space.World ? _target.rotation : _target.localRotation;
        }

        /// <inheritdoc/>
        protected override void Apply(float eased) {
            if (_target == null) {
                return;
            }

            var value = Quaternion.SlerpUnclamped(_from, _to, eased);
            if (_space == Space.World) {
                _target.rotation = value;
                return;
            }

            _target.localRotation = value;
        }

        /// <inheritdoc/>
        protected override void OnResetTweener() {
            _target = null!;
            _from = Quaternion.identity;
            _to = Quaternion.identity;
            _space = Space.World;
        }
    }
}
