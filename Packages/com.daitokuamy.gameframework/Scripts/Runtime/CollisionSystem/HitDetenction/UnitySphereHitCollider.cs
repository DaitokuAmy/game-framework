using UnityEngine;

namespace GameFramework.CollisionSystem {
    /// <summary>
    /// UnityのSphereColliderベースのHitCollider
    /// </summary>
    public sealed class UnitySphereHitCollider : ISphereHitCollider {
        private readonly SphereCollider _collider;

        /// <inheritdoc/>
        Vector3 ISphereHitCollider.Center => _collider.transform.TransformPoint(_collider.center);
        /// <inheritdoc/>
        float ISphereHitCollider.Radius => _collider.radius;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UnitySphereHitCollider(SphereCollider collider) {
            _collider = collider;
        }
    }
}
