using System;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// アクターの接近タイプ
    /// </summary>
    public enum ActorApproachType {
        None,
        Dash,
        RootScale,
        Teleport,
    }
    
    /// <summary>
    /// 接近移動用のパラメータ
    /// </summary>
    [Serializable]
    public struct ApproachParam {
        [Tooltip("接近タイプ")]
        public ActorApproachType type;
        [Tooltip("基本となるRoot移動量")]
        public Vector3 baseRootMovement;
        [Tooltip("間合い調整時の最小距離(負の値だと無効)")]
        public float minRange;
        [Tooltip("間合い調整時の最大距離(負の値だと無効)")]
        public float maxRange;
    }
}