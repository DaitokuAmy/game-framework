using System;
using SampleGame.Domain.ModelViewer;
using UnityEngine;
using IMeshAvatarInfo = SampleGame.Domain.ModelViewer.IPreviewActorMaster.IMeshAvatarInfo;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// プレビュー用のActorデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_preview_actor_setup_hoge.asset", menuName = "SampleGame/Model Viewer/Preview Actor Setup Data")]
    public class PreviewActorSetupData : ScriptableObject, IPreviewActorMaster {
        /// <summary>
        /// メッシュアバター情報
        /// </summary>
        [Serializable]
        public class MeshAvatarInfo : IMeshAvatarInfo {
            [Tooltip("アバターキー")]
            public string key;
            [Tooltip("アタッチするLocatorName")]
            public string locatorName;
            [Tooltip("デフォルトのアバターキー")]
            public int defaultIndex = 0;
            [Tooltip("Mesh用Prefab")]
            public GameObject[] prefabs;

            string IMeshAvatarInfo.Key => key;
            string IMeshAvatarInfo.LocatorName => locatorName;
            int IMeshAvatarInfo.DefaultIndex => defaultIndex;
            GameObject[] IMeshAvatarInfo.Prefabs => prefabs;
        }
        
        [Tooltip("Body用のPrefab")]
        public GameObject prefab;
        [Tooltip("初期のアニメーションクリップIndex")]
        public int defaultAnimationClipIndex;
        [Tooltip("アニメーションクリップリスト")]
        public AnimationClip[] animationClips = Array.Empty<AnimationClip>();
        [Tooltip("アバターメッシュ情報リスト")]
        public MeshAvatarInfo[] meshAvatarInfos = Array.Empty<MeshAvatarInfo>();

        public string Name => name.Replace("dat_preview_actor_setup_", "");
        public GameObject Prefab => prefab;
        public int DefaultAnimationClipIndex => defaultAnimationClipIndex;
        public AnimationClip[] AnimationClips => animationClips;
        public IMeshAvatarInfo[] MeshAvatarInfos => meshAvatarInfos;
    }
}
