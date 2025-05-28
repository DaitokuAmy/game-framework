using GameFramework.CoroutineSystems;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
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
        /// <summary>所属しているUIServiceへの参照</summary>
        protected UIService Service { get; private set; }
        /// <summary>現在更新中のDeltaTime</summary>
        protected float DeltaTime { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IUIView.Initialize(UIService service) {
            if (_disposed || _initialized) {
                return;
            }

            if (service == null) {
                Debug.LogError($"Not found service instance. [{this.name}]");
                return;
            }
            
            Service = service;
            RectTransform = (RectTransform)transform;

            // Awakeがコールされていない場合はInitializeを実行しない
            if (!_awaked) {
                return;
            }

            _initialized = true;
            
            _scope = new DisposableScope();
            _coroutineRunner = new CoroutineRunner();

            InitializeInternal(_scope);
            
            Service.RegisterView(this);
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
            
            if (Service != null) {
                Service.UnregisterView(this);
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
            DeltaTime = deltaTime;
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
        /// UIViewが存在するService管理化としてUIViewを生成する
        /// </summary>
        protected T InstantiateView<T>(T origin, Transform parent, bool worldPositionSpace = false)
            where T : UIView {
            var instance = Instantiate(origin.gameObject, parent, worldPositionSpace);
            var views = instance.GetComponentsInChildren<UIView>(true);
            foreach (var view in views) {
                ((IUIView)view).Initialize(Service);
            }

            var foundView = views.OfType<T>().FirstOrDefault();
            if (foundView == null) {
                Debug.LogError($"Not found instantiate view component. [{typeof(T).Name}]");
                return default;
            }

            return foundView;
        }

        /// <summary>
        /// マニュアル初期化（InstantiateViewを呼べない場合に使用してください）
        /// </summary>
        protected void ManualInitialize(GameObject target) {
            var views = target.GetComponentsInChildren<UIView>(true);
            foreach (var view in views) {
                ((IUIView)view).Initialize(Service);
            }
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
            if (Service != null) {
                var view = (IUIView)this;
                view.Initialize(Service);
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