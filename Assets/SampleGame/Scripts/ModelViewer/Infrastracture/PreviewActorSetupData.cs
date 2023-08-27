using System;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// プレビュー用のActorデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_preview_actor_setup_hoge.asset", menuName = "SampleGame/Model Viewer/Preview Actor Setup Data")]
    public class PreviewActorSetupData : ScriptableObject {
        /// <summary>
        /// メッシュアバター情報
        /// </summary>
        [Serializable]
        public class MeshAvatarInfo {
            [Tooltip("アバターキー")]
            public string key;
            [Tooltip("デフォルトのアバターキー")]
            public int defaultIndex = 0;
            [Tooltip("Mesh用Prefab")]
            public GameObject[] prefabs;
        }
        
        [Tooltip("Body用のPrefab")]
        public GameObject prefab;
        [Tooltip("アニメーションクリップリスト")]
        public AnimationClip[] animationClips = Array.Empty<AnimationClip>();
        [Tooltip("アバターメッシュ情報リスト")]
        public MeshAvatarInfo[] meshAvatarInfos = Array.Empty<MeshAvatarInfo>();
    }
}
