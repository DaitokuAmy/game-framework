using ActionSequencer;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// 攻撃判定範囲シーケンスイベント
    /// </summary>
    public class AttackRangeSequenceEvent : RangeSequenceEvent {
        [Tooltip("オフセット座標")]
        public Vector3 OffsetPositon;
        [Tooltip("半径")]
        public float Radius;
        [Tooltip("追従するか")]
        public bool Constraint;
    }
}