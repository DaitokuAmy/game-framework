using GameFramework.ActorSystems;
using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// PreviewActor制御用のController
    /// </summary>
    public class ActorController : ActorEntityLogic, IActorController {
        private readonly IReadOnlyActorModel _model;
        private readonly PreviewActor _actor;
        
        /// <summary>位置</summary>
        Vector3 IActorController.Position => _actor.Body.Position;
        /// <summary>向き</summary>
        Quaternion IActorController.Rotation => _actor.Body.Rotation;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorController(IReadOnlyActorModel model, PreviewActor actor) {
            _model = model;
            _actor = actor;
        }

        /// <summary>
        /// アニメーションクリップ設定
        /// </summary>
        void IActorController.ChangeAnimationClip(AnimationClip clip) {
            _actor.ChangeMotion(clip);
        }

        /// <summary>
        /// 加算アニメーションクリップ設定
        /// </summary>
        void IActorController.ChangeAdditiveAnimationClip(AnimationClip clip) {
            _actor.ChangeAdditiveMotion(clip);
        }

        /// <summary>
        /// アバターの変更
        /// </summary>
        void IActorController.ChangeMeshAvatar(string key, GameObject prefab, string locatorName) {
            _actor.ChangeMeshAvatar(key, prefab, locatorName);
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        void IActorController.ResetActor() {
            _actor.ChangeMotion(null);
            _actor.ChangeAdditiveMotion(null);
            _actor.ResetTransform();
        }
    }
}