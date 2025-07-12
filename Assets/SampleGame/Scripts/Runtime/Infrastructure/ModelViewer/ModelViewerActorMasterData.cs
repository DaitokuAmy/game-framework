using System;
using System.Collections.Generic;
using SampleGame.Domain.ModelViewer;
using UnityEngine;
using IMeshAvatarInfo = SampleGame.Domain.ModelViewer.IActorMaster.IMeshAvatarInfo;

namespace SampleGame.Infrastructure.ModelViewer {
    /// <summary>
    /// ModelViewer用のアクターマスターデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_model_viewer_actor_master_hoge.asset", menuName = "SampleGame/Model Viewer/Actor Master Data")]
    public class ModelViewerActorMasterData : ScriptableObject, IActorMaster {
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
            IReadOnlyList<GameObject> IMeshAvatarInfo.Prefabs => prefabs;
        }

        [Tooltip("表示名")]
        public string displayName;
        [Tooltip("Body用のPrefab")]
        public GameObject prefab;
        [Tooltip("初期のアニメーションクリップIndex")]
        public int defaultAnimationClipIndex;
        [Tooltip("アニメーションクリップリスト")]
        public AnimationClip[] animationClips = Array.Empty<AnimationClip>();
        [Tooltip("アバターメッシュ情報リスト")]
        public MeshAvatarInfo[] meshAvatarInfos = Array.Empty<MeshAvatarInfo>();

        string IActorMaster.DisplayName => string.IsNullOrEmpty(displayName) ? name.Replace("dat_model_viewer_actor_master_", "") : displayName;
        GameObject IActorMaster.Prefab => prefab;
        int IActorMaster.DefaultAnimationClipIndex => defaultAnimationClipIndex;
        IReadOnlyList<AnimationClip> IActorMaster.AnimationClips => animationClips;
        IReadOnlyList<IMeshAvatarInfo> IActorMaster.MeshAvatarInfos => meshAvatarInfos;
    }
}
