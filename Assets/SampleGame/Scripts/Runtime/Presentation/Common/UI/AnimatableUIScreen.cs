using System.Collections;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Animation機能付きのUIScreen
    /// </summary>
    public class AnimatableUIScreen : UIScreen {
        [Header("アニメーション")]
        [SerializeField, Tooltip("開くアニメーション")]
        private UIAnimationComponent _openAnimation;
        [SerializeField, Tooltip("閉じるアニメーション")]
        private UIAnimationComponent _closeAnimation;
        [SerializeField, Tooltip("戻りアニメーション(未指定ならCloseが使用される)")]
        private UIAnimationComponent _backAnimation;

        private UIAnimationPlayer _animationPlayer;

        /// <summary>アニメーションプレイヤー</summary>
        protected UIAnimationPlayer AnimationPlayer => _animationPlayer;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);
            _animationPlayer = new UIAnimationPlayer().ScopeTo(scope);
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            base.LateUpdateInternal(deltaTime);
            _animationPlayer.Update(deltaTime);
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected override IEnumerator OpenRoutine(TransitionType transitionType, IScope cancelScope) {
            yield return base.OpenRoutine(transitionType, cancelScope);
            yield return _animationPlayer.Play(_openAnimation);
        }

        /// <summary>
        /// 開く後処理
        /// </summary>
        protected override void PostOpen(TransitionType transitionType) {
            base.PostOpen(transitionType);
            _animationPlayer.Skip(_openAnimation);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected override IEnumerator CloseRoutine(TransitionType transitionType, IScope cancelScope) {
            yield return base.CloseRoutine(transitionType, cancelScope);
            yield return _animationPlayer.Play(GetCloseAnimation(transitionType));
        }

        /// <summary>
        /// 閉じる後処理
        /// </summary>
        protected override void PostClose(TransitionType transitionType) {
            base.PostOpen(transitionType);
            _animationPlayer.Skip(GetCloseAnimation(transitionType));
        }

        /// <summary>
        /// CloseAnimationの取得
        /// </summary>
        private IUIAnimation GetCloseAnimation(TransitionType transitionType) {
            var closeAnimation = _closeAnimation;
            if (transitionType == TransitionType.Back) {
                if (_backAnimation != null) {
                    closeAnimation = _backAnimation;
                }
            }

            return closeAnimation;
        }
    }
}