using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// 移動対象を表すターゲット情報
    /// </summary>
    public readonly struct MoveTarget {
        /// <summary>座標の基準</summary>
        public readonly Transform Transform;
        /// <summary>Transformがnullの場合はワールド座標、Transformが設定されている場合は相対座標</summary>
        public readonly Vector3 Point;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveTarget(Transform transform, Vector3 point) {
            Transform = transform;
            Point = point;
        }

        /// <summary>
        /// 絶対座標指定によるMoveTarget生成
        /// </summary>
        public static MoveTarget FromWorld(Vector3 worldPosition) => new(null, worldPosition);

        /// <summary>
        /// 相対座標指定によるMoveTarget生成
        /// </summary>
        public static MoveTarget FromRelative(Transform transform, Vector3 offset) => new(transform, offset);
        
        /// <summary>
        /// Transform指定によるMoveTarget生成
        /// </summary>
        public static MoveTarget FromTransform(Transform transform) => new(transform, Vector3.zero);

        /// <summary>
        /// ワールド座標の取得
        /// </summary>
        public Vector3 GetWorldPosition() {
            return Transform == null ? Point : Transform.position + Point;
        }
    }
}