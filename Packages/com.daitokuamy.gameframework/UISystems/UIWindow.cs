using GameFramework.CoroutineSystems;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GameFramework.Core;
using Coroutine = GameFramework.CoroutineSystems.Coroutine;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIWindowクラス
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIWindow : MonoBehaviour, IUIWindow {
        // 制御対象のUIViewリスト
        private readonly List<IUIView> _uiViews = new();
        // 登録リクエストされたUIView
        private readonly List<IUIView> _standbyUIViews = new();

        // 初期化済みフラグ
        private bool _initialized = false;
        // 廃棄済みフラグ
        private bool _disposed = false;
        // 子要素の変更フラグ
        private bool _dirtyChildren = false;
        // コルーチン再生用
        private CoroutineRunner _coroutineRunner;
        // 初期化用スコープ
        private DisposableScope _scope;

        /// <summary>更新時間用のTimeScale</summary>
        public float TimeScale { get; set; } = 1.0f;
        /// <summary>CanvasGroupの参照</summary>
        public CanvasGroup CanvasGroup { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IUIWindow.Initialize() {
            if (_disposed || _initialized) {
                return;
            }

            _initialized = true;
            _scope = new DisposableScope();
            _coroutineRunner = new CoroutineRunner();
            _dirtyChildren = true;

            CanvasGroup = GetComponent<CanvasGroup>();

            // すでに含まれているViewを登録する
            var views = GetComponentsInChildren<IUIView>(true);
            foreach (var view in views) {
                view.Initialize(this);
            }

            InitializeInternal(_scope);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (!_initialized || _disposed) {
                return;
            }

            DisposeInternal();

            _disposed = true;
            for (var i = _uiViews.Count - 1; i >= 0; i--) {
                var view = _uiViews[i];
                view.Dispose();
            }

            for (var i = _standbyUIViews.Count - 1; i >= 0; i--) {
                var view = _standbyUIViews[i];
                view.Dispose();
            }

            _uiViews.Clear();
            _standbyUIViews.Clear();
            _coroutineRunner.Dispose();
            _scope.Dispose();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IUIWindow.Update(float deltaTime) {
            RefreshUIViews();

            deltaTime *= TimeScale;

            _coroutineRunner.Update();

            UpdateInternal(deltaTime);

            foreach (var uiView in _uiViews) {
                uiView.Update(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IUIWindow.LateUpdate(float deltaTime) {
            RefreshUIViews();

            deltaTime *= TimeScale;

            LateUpdateInternal(deltaTime);

            foreach (var uiView in _uiViews) {
                uiView.LateUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Viewの登録
        /// </summary>
        internal void RegisterView(IUIView view) {
            if (_uiViews.Contains(view) || _standbyUIViews.Contains(view)) {
                return;
            }

            _standbyUIViews.Add(view);
            _dirtyChildren = true;
        }

        /// <summary>
        /// Viewの登録解除
        /// </summary>
        internal void UnregisterView(IUIView view) {
            _uiViews.Remove(view);
            _standbyUIViews.Remove(view);
        }

        /// <summary>
        /// 独自コルーチンの開始
        /// </summary>
        protected Coroutine StartCoroutine(IEnumerator enumerator, Action onCompleted = null,
            Action onCanceled = null, Action<Exception> onError = null, CancellationToken cancellationToken = default) {
            return _coroutineRunner.StartCoroutine(enumerator, onCompleted, onCanceled, onError, cancellationToken);
        }

        /// <summary>
        /// 独自コルーチンの停止
        /// </summary>
        protected void StopCoroutine(Coroutine coroutine) {
            _coroutineRunner.StopCoroutine(coroutine);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void LateUpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 子要素のUIViewをリフレッシュ
        /// </summary>
        private void RefreshUIViews() {
            if (!_dirtyChildren) {
                return;
            }

            _dirtyChildren = false;

            // Standby状態になっているViewをUIViewに登録
            foreach (var view in _standbyUIViews) {
                _uiViews.Add(view);
            }

            _standbyUIViews.Clear();

            // 全部Start処理を実行する
            foreach (var view in _uiViews) {
                view.Start();
            }
        }
    }
}