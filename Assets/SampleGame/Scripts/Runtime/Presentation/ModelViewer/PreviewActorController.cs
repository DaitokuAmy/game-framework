using GameFramework.ActorSystems;
using SampleGame.Domain.ModelViewer;
using UnityEngine;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// PreviewActor制御用のController
    /// </summary>
    public class PreviewActorController : ActorEntityLogic, IPreviewActorController {
        private readonly IReadOnlyPreviewActorModel _model;
        private readonly PreviewActor _actor;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorController(IReadOnlyPreviewActorModel model, PreviewActor actor) {
            _model = model;
            _actor = actor;
        }

        /// <summary>
        /// アニメーションクリップ設定
        /// </summary>
        void IPreviewActorController.ChangeAnimationClip(AnimationClip clip) {
            _actor.ChangeMotion(clip);
        }

        /// <summary>
        /// 加算アニメーションクリップ設定
        /// </summary>
        void IPreviewActorController.ChangeAdditiveAnimationClip(AnimationClip clip) {
            _actor.ChangeAdditiveMotion(clip);
        }

        /// <summary>
        /// アバターの変更
        /// </summary>
        void IPreviewActorController.ChangeMeshAvatar(string key, GameObject prefab, string locatorName) {
            _actor.ChangeMeshAvatar(key, prefab, locatorName);
        }

        /// <summary>
        /// アクターのリセット
        /// </summary>
        void IPreviewActorController.ResetActor() {
            _actor.ChangeMotion(null);
            _actor.ChangeAdditiveMotion(null);
            _actor.ResetTransform();
        }
    }
}