using System;
using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Mesh結合情報保持クラス
    /// </summary>
    [DisallowMultipleComponent]
    public class MeshParts : MonoBehaviour {
        // コンストレイント用のMask
        [Flags]
        public enum ConstraintMasks {
            Position = 1 << 0,
            Rotation = 1 << 1,
            LocalScale = 1 << 2,
        }

        // ユニーク骨情報
        [Serializable]
        public class UniqueBoneInfo {
            [Tooltip("AutoでConstraintするかのMask")]
            public ConstraintMasks constraintMask = 0;
            [Tooltip("ユニーク骨リスト")]
            public Transform[] targetBones = Array.Empty<Transform>();
        }

        [Tooltip("結合時に使用する骨のRoot(未指定だと結合元のRootと同名な物が使われる)")]
        public Transform boneRoot;
        [Tooltip("メッシュ結合時にユニーク制御する骨情報")]
        public UniqueBoneInfo[] uniqueBoneInfos = Array.Empty<UniqueBoneInfo>();
    }
}