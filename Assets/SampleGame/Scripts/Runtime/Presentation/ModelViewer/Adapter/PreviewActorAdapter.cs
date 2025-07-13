using GameFramework.ActorSystems;
using SampleGame.Domain.ModelViewer;
using ThirdPersonEngine.ModelViewer;
using UnityEngine;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// PreviewActor制御用のAdapter
    /// </summary>
    public class PreviewActorAdapter : ActorEntityLogic, IPreviewActorPort {
        private readonly IReadOnlyPreviewActorModel _model;
        private readonly PreviewActor _actor;
        
        /// <summary>位置</summary>
        Vector3 IPreviewActorPort.Position => _actor.Body.Position;
        /// <summary>向き</summary>
        Quaternion IPreviewActorPort.Rotation => _actor.Body.Rotation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorAdapter(IReadOnlyPreviewActorModel model, PreviewActor actor) {
            _model = model;
            _actor = actor;
        }

        /// <summary>
        /// アニメーションクリップ設定
        /// </summary>
        void IPreviewActorPort.ChangeAnimationClip(AnimationClip clip) {
            _actor.ChangeMotion(clip);
        }

        /// <summary>
        /// 加算アニメーションクリップ設定
        /// </summary>
        void IPreviewActorPort.ChangeAdditiveAnimationClip(AnimationClip clip) {
            _actor.ChangeAdditiveMotion(clip);
        }

        /// <summary>
        /// アバターの変更
        /// </summary>
        void IPreviewActorPort.ChangeMeshAvatar(string key, GameObject prefab, string locatorName) {
            _actor.ChangeMeshAvatar(key, prefab, locatorName);
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        void IPreviewActorPort.ResetActor() {
            _actor.ResetTransform();
        }
    }
}