using GameFramework.CoroutineSystems;
using UnityEngine;
using System;
using System.Collections;
using System.Threading;
using GameFramework.Core;
using Coroutine = GameFramework.CoroutineSystems.Coroutine;

namespace GameFramework.UISystems {
    /// <summary>
    /// UIViewクラス
    /// </summary>
    public class UIView : MonoBehaviour, IUIView {
        // Awakeフラグ
        private bool _awaked = false;
        // 初期化済みフラグ
        private bool _initialized = false;
        // 開始済みフラグ
        private bool _started = false;
        // 廃棄済みフラグ
        private bool _disposed = false;
        // コルーチン再生用
        private CoroutineRunner _coroutineRunner;
        // 初期化用スコープ
        private DisposableScope _scope;

        /// <summary>アクティブな状態か</summary>
        public virtual bool IsActive => isActiveAndEnabled;
        /// <summary>RectTransform</summary>
        public RectTransform RectTransform { get; private set; }
        /// <summary>所属しているUIWindowへの参照</summary>
        protected UIWindow Window { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IUIView.Initialize(UIWindow window) {
            if (_disposed || _initialized) {
                return;
            }

            if (window == null) {
                Debug.LogError($"Not found window instance. [{this.name}]");
                return;
            }
            
            Window = window;
            RectTransform = (RectTransform)transform;

            // Awakeがコールされていない場合はInitializeを実行しない
            if (!_awaked) {
                return;
            }

            _initialized = true;
            
            _scope = new DisposableScope();
            _coroutineRunner = new CoroutineRunner();

            InitializeInternal(_scope);
            
            Window.RegisterView(this);
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        void IUIView.Start() {
            if (_disposed || _started) {
                return;
            }

            if (!_initialized) {
                return;
            }

            _started = true;

            StartInternal(_scope);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            if (!_initialized || _disposed) {
                return;
            }
            
            if (Window != null) {
                Window.UnregisterView(this);
            }

            _disposed = true;

            DisposeInternal();
            
            _coroutineRunner.Dispose();
            _scope.Dispose();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IUIView.Update(float deltaTime) {
            _coroutineRunner.Update();

            if (IsActive) {
                UpdateInternal(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IUIView.LateUpdate(float deltaTime) {
            if (IsActive) {
                LateUpdateInternal(deltaTime);
            }
        }

        /// <summary>
        /// UIViewが存在するWindow管理化としてUIViewを生成する
        /// </summary>
        protected T InstantiateView<T>(T origin, Transform parent, bool worldPositionSpace = false)
            where T : UIView {
            var instance = Instantiate(origin.gameObject, parent, worldPositionSpace);
            var views = instance.GetComponentsInChildren<T>(true);
            foreach (var view in views) {
                ((IUIView)view).Initialize(Window);
            }

            if (views.Length <= 0) {
                Debug.LogError($"Not found instantiate view component. [{typeof(T).Name}]");
                return default;
            }

            return views[0];
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
        /// 開始時処理
        /// </summary>
        protected virtual void StartInternal(IScope scope) {
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
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _awaked = true;
            
            // Initializeの後に呼び出されている可能性があるため、初期化を改めて実行
            if (Window != null) {
                var view = (IUIView)this;
                view.Initialize(Window);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            ((IDisposable)this).Dispose();
        }
    }
}