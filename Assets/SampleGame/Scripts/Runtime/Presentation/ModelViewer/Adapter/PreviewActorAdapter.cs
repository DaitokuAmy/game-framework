using GameFramework.ActorSystems;
using SampleGame.Domain.ModelViewer;
using ThirdPersonEngine.ModelViewer;
using Unity.Mathematics;
using UnityEngine;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// PreviewActor制御用のAdapter
    /// </summary>
    public class PreviewActorAdapter : ActorEntityLogic, IPreviewActorPort {
        private readonly IReadOnlyPreviewActorModel _model;
        private readonly PreviewActor _actor;
        
        /// <summary>位置</summary>
        float3 IPreviewActorPort.Position => _actor.Body.Position;
        /// <summary>向き</summary>
        quaternion IPreviewActorPort.Rotation => _actor.Body.Rotation;
        /// <summary>存在するAnimationClip数</summary>
        int IPreviewActorPort.AnimationClipCount => _actor.GetMotionClips().Length;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorAdapter(IReadOnlyPreviewActorModel model, PreviewActor actor) {
            _model = model;
            _actor = actor;
        }

        /// <inheritdoc/>
        int IPreviewActorPort.GetDefaultAnimationClipIndex() {
            return _actor.GetDefaultMotionIndex();
        }

        /// <inheritdoc/>
        int IPreviewActorPort.GetDefaultMeshAvatarIndex(string key) {
            return _actor.GetDefaultMeshAvatarIndex(key);
        }

        /// <inheritdoc/>
        string[] IPreviewActorPort.GetMeshAvatarKeys() {
            return _actor.GetMeshAvatarKeys();
        }

        /// <inheritdoc/>
        int IPreviewActorPort.GetMeshAvatarCount(string key) {
            return _actor.GetMeshAvatarCount(key);
        }

        /// <inheritdoc/>
        void IPreviewActorPort.ChangeAnimationClip(int index) {
            _actor.ChangeMotion(index);
        }

        /// <inheritdoc/>
        void IPreviewActorPort.ChangeAdditiveAnimationClip(int index) {
            _actor.ChangeAdditiveMotion(index);
        }

        /// <inheritdoc/>
        void IPreviewActorPort.ChangeMeshAvatar(string key, int index) {
            _actor.ChangeMeshAvatar(key, index);
        }

        /// <inheritdoc/>
        void IPreviewActorPort.ResetActor() {
            _actor.ResetTransform();
        }
    }
}