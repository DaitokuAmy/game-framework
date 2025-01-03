using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// プレビュー用のActorデータのインタフェース
    /// </summary>
    public interface IPreviewActorMaster {
        /// <summary>
        /// メッシュアバター情報のインタフェース
        /// </summary>
        public interface IMeshAvatarInfo {
            /// <summary>アバターキー</summary>
            string Key { get; }
            /// <summary>アタッチするロケーター名</summary>
            string LocatorName { get; }
            /// <summary>デフォルトのアバターキー</summary>
            int DefaultIndex { get; }
            /// <summary>Mesh用Prefab</summary>
            GameObject[] Prefabs { get; }
        }

        /// <summary>名前</summary>
        string Name { get; }
        /// <summary>Body用のPrefab</summary>
        GameObject Prefab { get; }
        /// <summary>デフォルトのアニメーションクリップIndex</summary>
        int DefaultAnimationClipIndex { get; }
        /// <summary>アニメーションクリップリスト</summary>
        AnimationClip[] AnimationClips { get; }
        /// <summary>アバターメッシュ情報リスト</summary>
        IMeshAvatarInfo[] MeshAvatarInfos { get; }
    }
}