using System;
using UnityEngine;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// Vfx再生に使用するContext
    /// </summary>
    [Serializable]
    public struct VfxContext {
        [Tooltip("再生対象のPrefab")]
        public GameObject prefab;
        [Tooltip("基準Transformからの相対座標")]
        public Vector3 relativePosition;
        [Tooltip("座標を毎フレーム更新するか")]
        public bool constraintPosition;
        [Tooltip("基準Transformからの相対角度")]
        public Vector3 relativeAngles;
        [Tooltip("回転を毎フレーム更新するか")]
        public bool constraintRotation;
        [Tooltip("スケール")]
        public Vector3 localScale;
    }
}