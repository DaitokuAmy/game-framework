using UnityEngine;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// UnityのCapsuleColliderベースのHitCollider
    /// </summary>
    public sealed class UnitySweepHitCollider : ISweepHitCollider {
        private readonly CapsuleCollider _collider;

        /// <inheritdoc/>
        Vector3 ISweepHitCollider.Start => GetWorldCapsule().start;
        /// <inheritdoc/>
        Vector3 ISweepHitCollider.End => GetWorldCapsule().end;
        /// <inheritdoc/>
        float ISweepHitCollider.Radius => GetWorldCapsule().radius;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UnitySweepHitCollider(CapsuleCollider collider) {
            _collider = collider;
        }

        /// <summary>
        /// ワールド空間のカプセル情報の計算
        /// </summary>
        private (Vector3 start, Vector3 end, float radius) GetWorldCapsule() {
            var t = _collider.transform;

            // 方向（ローカル軸）
            var localAxis = _collider.direction switch {
                0 => Vector3.right,   // X
                1 => Vector3.up,      // Y
                2 => Vector3.forward, // Z
                _ => Vector3.up,
            };

            // Scale（負スケールも考慮）
            var s = t.lossyScale;
            var ax = Mathf.Abs(s.x);
            var ay = Mathf.Abs(s.y);
            var az = Mathf.Abs(s.z);

            // direction軸のスケール（高さ用）
            var axisScale = _collider.direction switch {
                0 => ax,
                1 => ay,
                2 => az,
                _ => ay,
            };

            // 垂直2軸の最大スケール（半径用：Unity/PhysX寄せの定番）
            var perpScale = _collider.direction switch {
                0 => Mathf.Max(ay, az),
                1 => Mathf.Max(ax, az),
                2 => Mathf.Max(ax, ay),
                _ => Mathf.Max(ax, az),
            };

            var worldRadius = _collider.radius * perpScale;
            var worldHeight = _collider.height * axisScale;

            // カプセルの“円柱部分”半長（height < 2r なら0）
            var halfSegment = Mathf.Max(0.0f, worldHeight * 0.5f - worldRadius);

            // center（ワールド）
            var centerWorld = t.TransformPoint(_collider.center);

            // axis（ワールド：非一様スケールでTransformDirectionが伸びるので正規化）
            var axisWorld = t.TransformDirection(localAxis);
            var axisLenSq = axisWorld.sqrMagnitude;
            if (axisLenSq > 1e-20f) {
                axisWorld /= Mathf.Sqrt(axisLenSq);
            }
            else {
                axisWorld = Vector3.up;
            }

            var start = centerWorld - axisWorld * halfSegment;
            var end = centerWorld + axisWorld * halfSegment;

            return (start, end, worldRadius);
        }
    }
}