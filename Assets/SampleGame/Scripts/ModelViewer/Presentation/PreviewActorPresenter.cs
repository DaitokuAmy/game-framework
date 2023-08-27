using GameFramework.Core;
using GameFramework.ActorSystems;
using UniRx;
using UnityEngine.InputSystem;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// PreviewActor制御用のPresenter
    /// </summary>
    public class PreviewActorPresenter : ActorEntityLogic {
        private PreviewActorModel _model;
        private PreviewCharacterActor _characterActor;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActorPresenter(PreviewActorModel model, PreviewCharacterActor characterActor) {
            _model = model;
            _characterActor = characterActor;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            // モーションの切り替え
            _model.CurrentAnimationClip
                .TakeUntil(scope)
                .Subscribe(clip => { _characterActor.ChangeMotion(clip); });
            _model.CurrentAdditiveAnimationClip
                .TakeUntil(scope)
                .Subscribe(clip => { _characterActor.ChangeAdditiveMotion(clip); });

            // アバターの切り替え
            _model.CurrentMeshAvatars
                .ObserveAdd()
                .TakeUntil(scope)
                .Subscribe(pair => { _characterActor.ChangeMeshAvatar(pair.Key, pair.Value); });
            _model.CurrentMeshAvatars
                .ObserveRemove()
                .TakeUntil(scope)
                .Subscribe(pair => { _characterActor.ChangeMeshAvatar(pair.Key, null); });
            _model.CurrentMeshAvatars
                .ObserveReplace()
                .TakeUntil(scope)
                .Subscribe(pair => { _characterActor.ChangeMeshAvatar(pair.Key, pair.NewValue); });

            foreach (var pair in _model.CurrentMeshAvatars) {
                _characterActor.ChangeMeshAvatar(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            // モーションの再適用
            if (Keyboard.current[Key.Space].wasPressedThisFrame) {
                _characterActor.ChangeMotion(_model.CurrentAnimationClip.Value);
            }
            
            // モーションのIndex更新
            if (Keyboard.current[Key.UpArrow].wasPressedThisFrame) {
                _model.ChangeClip((_model.CurrentAnimationClipIndex - 1 + _model.AnimationClipCount) % _model.AnimationClipCount);
            }
            if (Keyboard.current[Key.DownArrow].wasPressedThisFrame) {
                _model.ChangeClip((_model.CurrentAnimationClipIndex + 1) % _model.AnimationClipCount);
            }
        }
    }
}