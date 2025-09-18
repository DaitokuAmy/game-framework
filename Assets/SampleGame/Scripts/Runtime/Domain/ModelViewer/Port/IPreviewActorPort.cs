using System.Collections.Generic;
using Unity.Mathematics;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// プレビュー用のActor制御クラス
    /// </summary>
    public interface IPreviewActorPort {
        /// <summary>位置</summary>
        float3 Position { get; }
        /// <summary>向き</summary>
        quaternion Rotation { get; }
        /// <summary>存在するAnimationClip数</summary>
        int AnimationClipCount { get; }

        /// <summary>
        /// MeshAvatarのDefaultIndexを取得
        /// </summary>
        int GetDefaultMeshAvatarIndex(string key);

        /// <summary>
        /// AnimationClipのDefaultIndexを取得
        /// </summary>
        int GetDefaultAnimationClipIndex();

        /// <summary>
        /// MeshAvatarのキー一覧を取得
        /// </summary>
        string[] GetMeshAvatarKeys();

        /// <summary>
        /// MeshAvatarの切り替え可能数を取得
        /// </summary>
        int GetMeshAvatarCount(string key);
        
        /// <summary>
        /// AnimationClipの切り替え
        /// </summary>
        void ChangeAnimationClip(int index);
        
        /// <summary>
        /// 加算AnimationClipの切り替え
        /// </summary>
        void ChangeAdditiveAnimationClip(int index);
        
        /// <summary>
        /// メッシュアバターの切り替え
        /// </summary>
        void ChangeMeshAvatar(string key, int index);
        
        /// <summary>
        /// 状態リセット
        /// </summary>
        void ResetActor();
    }
}