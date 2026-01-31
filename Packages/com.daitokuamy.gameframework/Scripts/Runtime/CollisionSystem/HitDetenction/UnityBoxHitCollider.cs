using UnityEngine;

namespace GameFramework.CollisionSystem {
    /// <summary>
    /// UnityのBoxColliderベースのHitCollider
    /// </summary>
    public sealed class UnityBoxHitCollider : IBoxHitCollider {
        private readonly BoxCollider _collider;

        /// <inheritdoc/>
        Vector3 IBoxHitCollider.Center => _collider.transform.TransformPoint(_collider.center);
        /// <inheritdoc/>
        Quaternion IBoxHitCollider.Rotation => _collider.transform.rotation;
        /// <inheritdoc/>
        Vector3 IBoxHitCollider.HalfExtents => _collider.size;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UnityBoxHitCollider(BoxCollider collider) {
            _collider = collider;
        }
    }
}