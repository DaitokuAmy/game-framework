using GameFramework.ActorSystems;
using SampleGame.Domain.ModelViewer;
using ThirdPersonEngine.ModelViewer;
using UnityEngine;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// PreviewActor制御用のAdapter
    /// </summary>
    public class ActorAdapter : ActorEntityLogic, IActorPort {
        private readonly IReadOnlyPreviewActorModel _model;
        private readonly PreviewActor _actor;
        
        /// <summary>位置</summary>
        Vector3 IActorPort.Position => _actor.Body.Position;
        /// <summary>向き</summary>
        Quaternion IActorPort.Rotation => _actor.Body.Rotation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorAdapter(IReadOnlyPreviewActorModel model, PreviewActor actor) {
            _model = model;
            _actor = actor;
        }

        /// <summary>
        /// アニメーションクリップ設定
        /// </summary>
        void IActorPort.ChangeAnimationClip(AnimationClip clip) {
            _actor.ChangeMotion(clip);
        }

        /// <summary>
        /// 加算アニメーションクリップ設定
        /// </summary>
        void IActorPort.ChangeAdditiveAnimationClip(AnimationClip clip) {
            _actor.ChangeAdditiveMotion(clip);
        }

        /// <summary>
        /// アバターの変更
        /// </summary>
        void IActorPort.ChangeMeshAvatar(string key, GameObject prefab, string locatorName) {
            _actor.ChangeMeshAvatar(key, prefab, locatorName);
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        void IActorPort.ResetActor() {
            _actor.ResetTransform();
        }
    }
}