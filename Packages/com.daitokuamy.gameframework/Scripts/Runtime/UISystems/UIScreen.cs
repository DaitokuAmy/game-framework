using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIScreenクラス（遷移など行う際に使うコンテンツ単位）
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : UIView {
        /// <summary>
        /// オープン状態
        /// </summary>
        public enum OpenStatus {
            Invalid = -1,
            Opening,
            Opened,
            Closing,
            Closed,
        }

        [SerializeField, Tooltip("開始時に開いた状態にするか")]
        private bool _openOnStart = true;

        private readonly List<IUIScreenHandler> _handlers = new();

        private DisposableScope _animationScope;
        private DisposableScope _activeScope;
        private AnimationStatus _currentAnimationStatus;
        private bool _dirtyOpenStatus;

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
        /// <summary>現在のOpenStatus</summary>
        public OpenStatus CurrentOpenStatus { get; private set; } = OpenStatus.Invalid;
        /// <summary>キャンバスグループ</summary>
        public CanvasGroup CanvasGroup { get; private set; }

        /// <summary>
        /// 開く処理
        /// </summary>
        /// <param name="transitionDirection">遷移方向</param>
        /// <param name="immediate">即時完了するか</param>
        /// <param name="force">既に開いている場合でも開きなおすか</param>
        public AnimationHandle OpenAsync(TransitionDirection transitionDirection = TransitionDirection.Forward, bool immediate = false, bool force = false) {
            var status = new AnimationStatus();
            var handle = new AnimationHandle(status);

            // 強制じゃない場合は既にOpenしていた場合は終わる
            if (!force) {
                if (CurrentOpenStatus == OpenStatus.Opening || CurrentOpenStatus == OpenStatus.Opened) {
                    if (_currentAnimationStatus != null) {
                        return new AnimationHandle(_currentAnimationStatus);
                    }

                    status.Complete();
                    return handle;
                }
            }

            _dirtyOpenStatus = true;
            _animationScope.Clear();
            _currentAnimationStatus = status;

            gameObject.SetActive(true);
            CurrentOpenStatus = OpenStatus.Opening;
            PreOpen(transitionDirection, immediate);
            foreach (var handler in _handlers) {
                handler.PreOpen();
            }

            void PostOpenInternal() {
                PostOpen(transitionDirection, immediate);
                foreach (var handler in _handlers) {
                    handler.PostOpen();
                }

                CurrentOpenStatus = OpenStatus.Opened;
                _currentAnimationStatus = null;
                Activate();
            }

            // 即時実行の場合は、Post処理だけ呼ぶ
            if (immediate) {
                PostOpenInternal();
                _animationScope.Clear();
                status.Complete();
            }
            else {
                StartCoroutine(OpenRoutine(transitionDirection, _animationScope),
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
        /// <param name="transitionDirection">遷移方向</param>
        /// <param name="immediate">即時完了するか</param>
        /// <param name="force">既に閉じている場合でも閉じなおすか</param>
        public AnimationHandle CloseAsync(TransitionDirection transitionDirection = TransitionDirection.Forward, bool immediate = false, bool force = false) {
            var status = new AnimationStatus();
            var handle = new AnimationHandle(status);

            // 既にActiveじゃないならそのまま終わる
            if (!IsActive) {
                status.Complete();
                return handle;
            }

            // 強制じゃない場合は既にCloseしていた場合は終わる
            if (!force) {
                if (CurrentOpenStatus == OpenStatus.Closing || CurrentOpenStatus == OpenStatus.Closed) {
                    if (_currentAnimationStatus != null) {
                        return new AnimationHandle(_currentAnimationStatus);
                    }

                    status.Complete();
                    return handle;
                }
            }

            _dirtyOpenStatus = true;
            _animationScope.Clear();
            _currentAnimationStatus = status;

            Deactivate();
            CurrentOpenStatus = OpenStatus.Closing;
            PreClose(transitionDirection, immediate);
            foreach (var handler in _handlers) {
                handler.PreClose();
            }

            void PostCloseInternal() {
                PostClose(transitionDirection, immediate);
                foreach (var handler in _handlers) {
                    handler.PostClose();
                }

                CurrentOpenStatus = OpenStatus.Closed;
                _currentAnimationStatus = null;
                gameObject.SetActive(false);
            }

            // 即時実行の場合は、Post処理だけ呼ぶ
            if (immediate) {
                PostCloseInternal();
                _animationScope.Clear();
                status.Complete();
            }
            else {
                StartCoroutine(CloseRoutine(transitionDirection, _animationScope),
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
        /// 開始時処理
        /// </summary>
        protected override void StartInternal(IScope scope) {
            base.StartInternal(scope);

            if (!_dirtyOpenStatus) {
                if (_openOnStart) {
                    OpenAsync();
                }
                else {
                    CloseAsync(immediate: true);
                }
            }
        }

        /// <summary>
        /// 開く直前の処理（アニメーションを飛ばした場合も使用）
        /// </summary>
        protected virtual void PreOpen(TransitionDirection transitionDirection, bool immediate) {
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        protected virtual IEnumerator OpenRoutine(TransitionDirection transitionDirection, IScope cancelScope) {
            yield break;
        }

        /// <summary>
        /// 開いた直後の処理（アニメーションを飛ばした場合も使用）
        /// </summary>
        protected virtual void PostOpen(TransitionDirection transitionDirection, bool immediate) {
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
        protected virtual void PreClose(TransitionDirection transitionDirection, bool immediate) {
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected virtual IEnumerator CloseRoutine(TransitionDirection transitionDirection, IScope cancelScope) {
            yield break;
        }

        /// <summary>
        /// 閉じた直後の処理（アニメーションを飛ばした場合も使用）
        /// </summary>
        protected virtual void PostClose(TransitionDirection transitionDirection, bool immediate) {
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        protected override void DisposeInternal() {
            Deactivate();

            for (var i = _handlers.Count - 1; i >= 0; i--) {
                var handler = _handlers[i];
                UnregisterHandler(handler);
            }

            base.DisposeInternal();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            base.UpdateInternal(deltaTime);

            foreach (var handler in _handlers) {
                handler.Update(deltaTime);
            }
        }

        /// <summary>
        /// 後処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            base.LateUpdateInternal(deltaTime);

            foreach (var handler in _handlers) {
                handler.LateUpdate(deltaTime);
            }
        }

        /// <summary>
        /// ハンドラーの登録
        /// </summary>
        public void RegisterHandler(IUIScreenHandler handler) {
            if (_handlers.Contains(handler)) {
                return;
            }

            _handlers.Add(handler);
            handler.OnRegistered(this);
            
            // Open状態だった場合関数を呼び出しておく
            if (CurrentOpenStatus == OpenStatus.Opened || CurrentOpenStatus == OpenStatus.Closing) {
                handler.PreOpen();
                handler.PostOpen();
            }
            else if (CurrentOpenStatus == OpenStatus.Opening) {
                handler.PreOpen();
            }

            // Active状態だった場合関数を呼び出しておく
            if (IsActivated) {
                if (!handler.IsActive) {
                    handler.Activate();
                }
            }
            else {
                if (handler.IsActive) {
                    handler.Deactivate();
                }
            }
            
            // Close中だった場合必要な関数を呼び出しておく
            if (CurrentOpenStatus == OpenStatus.Closing) {
                handler.PreClose();
            }
        }

        /// <summary>
        /// ハンドラーの解除
        /// </summary>
        public void UnregisterHandler(IUIScreenHandler handler) {
            if (!_handlers.Contains(handler)) {
                return;
            }

            _handlers.Remove(handler);
            
            // 閉じていない場合関数を呼び出しておく
            if (CurrentOpenStatus == OpenStatus.Closing) {
                handler.PostClose();
            }
            else if (CurrentOpenStatus == OpenStatus.Opened) {
                handler.PreClose();
                handler.PostClose();
            }
            else if (CurrentOpenStatus == OpenStatus.Opening) {
                handler.PostOpen();
                handler.PreClose();
                handler.PostClose();
            }

            // Active状態だった場合関数を呼び出しておく
            if (handler.IsActive) {
                handler.Deactivate();
            }

            handler.OnRegistered(this);
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
            foreach (var handler in _handlers) {
                handler.Activate();
            }
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
            foreach (var handler in _handlers) {
                handler.Deactivate();
            }

            DeactivateInternal();
            _activeScope.Clear();
        }
    }
}