using UnityEngine;

namespace GameFramework.CollisionSystem {
    /// <summary>
    /// 衝突イベント情報
    /// </summary>
    public readonly struct HitEvent {
        /// <summary>Hit側のId</summary>
        public readonly int HitId;
        /// <summary>受け側のId</summary>
        public readonly int ReceiveId;
        /// <summary>衝突位置</summary>
        public readonly Vector3 ContactPoint;
        /// <summary>衝突向き</summary>
        public readonly Vector3 ContactNormal;
        /// <summary>Hit側に設定されたカスタムデータ</summary>
        public readonly object CustomData;

        public HitEvent(int hitId, int receiveId, Vector3 contactPoint, Vector3 contactNormal, object customData) {
            HitId = hitId;
            ReceiveId = receiveId;
            ContactPoint = contactPoint;
            ContactNormal = contactNormal;
            CustomData = customData;
        }
    }
}
