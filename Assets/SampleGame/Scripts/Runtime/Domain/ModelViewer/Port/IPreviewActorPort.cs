using UnityEngine;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// プレビュー用のActor制御クラス
    /// </summary>
    public interface IPreviewActorPort {
        /// <summary>位置</summary>
        Vector3 Position { get; }
        /// <summary>向き</summary>
        Quaternion Rotation { get; }
        
        /// <summary>
        /// AnimationClipの切り替え
        /// </summary>
        void ChangeAnimationClip(AnimationClip clip);
        
        /// <summary>
        /// 加算AnimationClipの切り替え
        /// </summary>
        void ChangeAdditiveAnimationClip(AnimationClip clip);
        
        /// <summary>
        /// メッシュアバターの切り替え
        /// </summary>
        void ChangeMeshAvatar(string key, GameObject prefab, string locatorName);
        
        /// <summary>
        /// 状態リセット
        /// </summary>
        void ResetActor();
    }
}