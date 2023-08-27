using UnityEngine;
using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIScreenクラス（遷移など行う際に使うコンテンツ単位）
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : UIView {
        // アニメーションキャンセル用スコープ
        private DisposableScope _animationScope;
        // アクティブ状態のスコープ
        private DisposableScope _activeScope;
        
        /// <summary>アクティブ化されているか</summary>
        public bool IsActivated { get; private set; }
        /// <summary>操作可能か</summary>
        public bool Interactable {
            get => CanvasGroup.interactable;
            set => CanvasGroup.interactable = value;
        }
        /// <summary>レイキャストブロックするか</summary>
        public bool BlocksRaycasts {
            get => CanvasGroup.blocksRaycasts;
            set => CanvasGroup.blocksRaycasts = value;
        }
        /// <summary>キャンバスグループ</summary>
        public CanvasGroup CanvasGroup { get; private set; }

        /// <summary>
        /// 開く処理
        /// </summary>
        public AnimationHandle OpenAsync(TransitionType transitionType, bool immediate) {
            var status = new AnimationStatus();
            var handle = new AnimationHandle(status);
            
            _animationScope.Clear();
            
            gameObject.SetActive(true);
            PreOpen(transitionType);

            void PostOpenInternal() {
                PostOpen(transitionType);
                Activate();
            }

            // 即時実行の場合は、Post処理だけ呼ぶ
            if (immediate) {
                PostOpenInternal();
                _animationScope.Clear();
                status.Complete();
            }
            else {
                StartCoroutine(OpenRoutine(transitionType, _animationScope),
                    onCompleted: () => {
                        PostOpenInternal();
                        _animationScope.Clear();
                        status.Complete();
                    },
                    onCanceled: () => {
                        PostOpenInternal();
                        _animationScope.Clear();
                        status.Complete();
                    },
                    onError: exception => {
                        _animationScope.Clear();
                        status.Abort(exception);
                    },
                    cancellationToken: _animationScope.Token);
            }

            return handle;
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        public AnimationHandle CloseAsync(TransitionType transitionType, bool immediate) {
            var status = new AnimationStatus();
            var handle = new AnimationHandle(status);
            
            _animationScope.Clear();
            
            Deactivate();
            PreClose(transitionType);

            void PostCloseInternal() {
                PostClose(transitionType);
                gameObject.SetActive(false);
            }

            // 即時実行の場合は、Post処理だけ呼ぶ
            if (immediate) {
                PostCloseInternal();
                _animationScope.Clear();
                status.Complete();
            }
            else {
                StartCoroutine(CloseRoutine(transitionType, _animationScope),
                    onCompleted: () => {
                        PostCloseInternal();
                        _animationScope.Clear();
                        status.Complete();
                    },
                    onCanceled: () => {
                        PostCloseInternal();
                        _animationScope.Clear();
                        status.Complete();
                    },
                    onError: exception => {
                        _animationScope.Clear();
                        status.Abort(exception);
                    },
                    cancellationToken: _animationScope.Token);
            }

            return handle;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);

            CanvasGroup = GetComponent<CanvasGroup>();
            _activeScope = new DisposableScope();
            _animationScope = new DisposableScope();
        }

        /// <summary>
        /// 開く直前の処理（アニメーションを飛ばした場合も使用）
        /// </summary>
        protected virtual void PreOpen(TransitionType transitionType) {
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected virtual IEnumerator OpenRoutine(TransitionType transitionType, IScope cancelScope) {
            yield break;
        }

        /// <summary>
        /// 開いた直後の処理（アニメーションを飛ばした場合も使用）
        /// </summary>
        protected virtual void PostOpen(TransitionType transitionType) {
        }

        /// <summary>
        /// アクティブ化された時の処理
        /// </summary>
        protected virtual void ActivateInternal(IScope scope) {
        }

        /// <summary>
        /// 非アクティブ化された時の処理
        /// </summary>
        protected virtual void DeactivateInternal() {
        }

        /// <summary>
        /// 閉じる直前の処理（アニメーションを飛ばした場合も使用）
        /// </summary>
        protected virtual void PreClose(TransitionType transitionType) {
        }
        
        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected virtual IEnumerator CloseRoutine(TransitionType transitionType, IScope cancelScope) {
            yield break;
        }

        /// <summary>
        /// 閉じた直後の処理（アニメーションを飛ばした場合も使用）
        /// </summary>
        protected virtual void PostClose(TransitionType transitionType) {
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        protected override void DisposeInternal() {
            Deactivate();
            base.DisposeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        private void Activate() {
            if (IsActivated) {
                return;
            }

            IsActivated = true;
            ActivateInternal(_activeScope);
        }

        /// <summary>
        /// 非アクティブ化処理
        /// </summary>
        private void Deactivate() {
            if (!IsActivated) {
                return;
            }

            IsActivated = false;
            _animationScope.Clear();
            DeactivateInternal();
            _activeScope.Clear();
        }
    }
}