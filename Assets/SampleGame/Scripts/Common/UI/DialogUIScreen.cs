using System;
using System.Collections;
using GameFramework.Core;
using GameFramework.UISystems;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SampleGame {
    /// <summary>
    /// ダイアログ用スクリーン
    /// </summary>
    public class DialogUIScreen : UIScreen {
        [SerializeField, Tooltip("決定ボタン")]
        private Button _decideButton;
        [SerializeField, Tooltip("キャンセルボタン")]
        private Button _cancelButton;
        [SerializeField, Tooltip("開閉に使うアニメーション")]
        private UIAnimationComponent _animation;

        private UIAnimationPlayer _animationPlayer;

        /// <summary>決定ボタン通知</summary>
        public IObservable<Unit> OnDecideButton => _decideButton.OnClickAsObservable();
        /// <summary>キャンセルボタン通知</summary>
        public IObservable<Unit> OnCancelButton => _cancelButton.OnClickAsObservable();

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            _animationPlayer = new UIAnimationPlayer();
            _animationPlayer.ScopeTo(scope);
        }

        /// <summary>
        /// 開くアニメーション
        /// </summary>
        protected override IEnumerator OpenRoutine(TransitionType transitionType, IScope cancelScope) {
            yield return _animationPlayer.Play(_animation);
        }

        /// <summary>
        /// 開くのが完了した時の処理
        /// </summary>
        /// <param name="transitionType"></param>
        protected override void PostOpen(TransitionType transitionType) {
            _animationPlayer.Skip(_animation);
        }

        /// <summary>
        /// 閉じるアニメーション
        /// </summary>
        protected override IEnumerator CloseRoutine(TransitionType transitionType, IScope cancelScope) {
            yield return _animationPlayer.Play(_animation, true);
        }

        /// <summary>
        /// 閉じるのが完了した時の処理
        /// </summary>
        /// <param name="transitionType"></param>
        protected override void PostClose(TransitionType transitionType) {
            _animationPlayer.Skip(_animation, true);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);
            _animationPlayer.Update(deltaTime);
        }
    }
}
