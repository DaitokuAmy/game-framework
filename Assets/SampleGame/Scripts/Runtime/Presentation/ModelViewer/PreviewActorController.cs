using GameFramework.Core;
using GameFramework.ActorSystems;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain.ModelViewer;
using UnityEngine;
using UnityEngine.InputSystem;

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

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            var appService = Services.Get<ModelViewerAppService>();
            appService.SetActorController(this);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            var appService = Services.Get<ModelViewerAppService>();

            // モーションの再適用
            if (Keyboard.current[Key.Space].wasPressedThisFrame) {
                _actor.ChangeMotion(_model.CurrentAnimationClip);
            }

            // モーションのIndex更新
            if (Keyboard.current[Key.UpArrow].wasPressedThisFrame) {
                appService.ChangeAnimationClip((_model.CurrentAnimationClipIndex - 1 + _model.AnimationClipCount) % _model.AnimationClipCount);
            }

            if (Keyboard.current[Key.DownArrow].wasPressedThisFrame) {
                appService.ChangeAnimationClip((_model.CurrentAnimationClipIndex + 1) % _model.AnimationClipCount);
            }
        }
    }
}