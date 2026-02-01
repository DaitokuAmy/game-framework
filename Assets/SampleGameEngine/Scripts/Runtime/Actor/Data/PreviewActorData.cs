using System;
using UnityEngine;

namespace SampleGameEngine {
    /// <summary>
    /// PreviewActor用の設定データ
    /// </summary> 
    [CreateAssetMenu(menuName = "Sample Game Engine/Actor Data/Preview Actor", fileName = "dat_act_ch000_00_preview.asset")]
    public sealed class PreviewActorData : ScriptableObject {
        /// <summary>
        /// アバターメッシュ情報
        /// </summary>
        [Serializable]
        public sealed class AvatarMeshInfo {
            [Tooltip("アバターキー")]
            public string key;
            [Tooltip("アタッチするLocatorName")]
            public string locatorName;
            [Tooltip("デフォルトのアバターキー")]
            public int defaultIndex = 0;
            [Tooltip("Mesh用Prefab")]
            public GameObject[] prefabs;
        }

        [Tooltip("Body用のPrefab")]
        public GameObject prefab;
        [Tooltip("初期のアニメーションクリップIndex")]
        public int defaultAnimationClipIndex;
        [Tooltip("アニメーションクリップリスト")]
        public AnimationClip[] animationClips = Array.Empty<AnimationClip>();
        [Tooltip("アバターメッシュ情報リスト")]
        public AvatarMeshInfo[] meshAvatarInfos = Array.Empty<AvatarMeshInfo>();
    }
}
