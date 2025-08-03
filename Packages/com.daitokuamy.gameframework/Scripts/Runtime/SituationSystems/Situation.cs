using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション
    /// </summary>
    public abstract class Situation : ISituation {
        /// <summary>
        /// 状態
        /// </summary>
        public enum State {
            Invalid = -1,
            Standby, // 待機状態
            Loading, // 読み込み中
            Loaded, // 読み込み済
            SetupFinished, // 初期化済
            Opening, // オープン中
            OpenFinished, // オープン済
        }

        private readonly List<Situation> _children = new();
        private readonly DisposableScope _loadScope;
        private readonly DisposableScope _setupScope;
        private readonly DisposableScope _activeScope;
        private readonly DisposableScope _openScope;
        private readonly DisposableScope _animationScope;
        private readonly DisposableScope _focusScope;

        private Situation _parent;
        private SituationContainer _container;

        /// <summary>UnitySceneを保持するSituationか</summary>
        public virtual bool HasScene => false;
        /// <summary>PreLoad可能か</summary>
        public virtual bool CanPreLoad => true;

        /// <summary>親のSituation</summary>
        ISituation ISituation.Parent => _parent;
        /// <summary>子Situationリスト</summary>
        IReadOnlyList<ISituation> ISituation.Children => _children;

        /// <summary>インスタンス管理用</summary>
        public ServiceContainer ServiceContainer { get; private set; }
        /// <summary>現在状態</summary>
        public State CurrentState { get; private set; } = State.Invalid;
        /// <summary>アクティブ状態か</summary>
        public bool IsActive { get; private set; }
        /// <summary>プリロード状態</summary>
        public PreLoadState PreLoadState { get; private set; } = PreLoadState.None;
        /// <summary>フォーカス状態</summary>
        public bool IsFocused { get; private set; }
        /// <summary>コンテナ返却プロパティ</summary>
        public SituationContainer Container => _container;
        /// <summary>RootSituationか</summary>
        public bool IsRoot => _parent == null;
        /// <summary>親のSituation</summary>
        public Situation Parent => _parent;
        /// <summary>子Situationリスト</summary>
        public IReadOnlyList<Situation> Children => _children;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Situation() {
            _loadScope = new();
            _setupScope = new();
            _activeScope = new();
            _openScope = new();
            _animationScope = new();
            _focusScope = new();
        }

        /// <inheritdoc/>
        void ISituation.Standby(SituationContainer container) {
            if (_container != null && _container != container) {
                Debug.LogError("Already exists container.");
                return;
            }

            if (CurrentState >= State.Standby) {
                return;
            }

            CurrentState = State.Standby;
            _container = container;
            StandbyInternal(_container);
        }

        /// <inheritdoc/>
        IEnumerator ISituation.LoadRoutine(TransitionHandle<Situation> handle, bool preload) {
            if (CurrentState >= State.Loaded) {
                yield break;
            }

            // PreLoadしている場合、Loadedになるのを待つ
            if (!preload && PreLoadState != PreLoadState.None) {
                while (CurrentState < State.Loaded) {
                    yield return null;
                }

                yield break;
            }

            ServiceContainer = new ServiceContainer(_parent?.ServiceContainer ?? Services.Instance, false, GetType().FullName);
            CurrentState = State.Loading;
            yield return LoadRoutineInternal(handle, _loadScope);
            CurrentState = State.Loaded;
        }

        /// <inheritdoc/>
        IEnumerator ISituation.SetupRoutine(TransitionHandle<Situation> handle) {
            if (CurrentState >= State.SetupFinished) {
                yield break;
            }

            yield return SetupRoutineInternal(handle, _setupScope);
            CurrentState = State.SetupFinished;
        }

        /// <inheritdoc/>
        void ISituation.Activate(TransitionHandle<Situation> handle) {
            if (IsActive) {
                return;
            }

            IsActive = true;
            ActivateInternal(handle, _activeScope);
        }

        /// <inheritdoc/>
        void ISituation.PreOpen(TransitionHandle<Situation> handle) {
            if (CurrentState >= State.Opening) {
                return;
            }

            PreOpenInternal(handle, _openScope);
            CurrentState = State.Opening;
        }

        /// <inheritdoc/>
        IEnumerator ISituation.OpenRoutine(TransitionHandle<Situation> handle) {
            yield return OpenRoutineInternal(handle, _animationScope);
            _animationScope.Clear();
        }

        /// <inheritdoc/>
        void ISituation.PostOpen(TransitionHandle<Situation> handle) {
            PostOpenInternal(handle, _openScope);
        }

        /// <inheritdoc/>
        void ISituation.Update() {
            if (CurrentState == State.Invalid) {
                return;
            }

            // Systemの更新
            SystemUpdateInternal();

            // アクティブ状態なら更新を実行
            if (IsActive) {
                UpdateInternal();
            }
        }

        /// <inheritdoc/>
        void ISituation.LateUpdate() {
            if (CurrentState == State.Invalid) {
                return;
            }

            // Systemの更新
            SystemLateUpdateInternal();

            // LateUpdateはActive中のみ
            if (CurrentState == State.OpenFinished) {
                LateUpdateInternal();
            }
        }

        /// <inheritdoc/>
        void ISituation.FixedUpdate() {
            if (CurrentState == State.Invalid) {
                return;
            }

            // Systemの更新
            SystemFixedUpdateInternal();

            // FixedUpdateはActive中のみ
            if (CurrentState == State.OpenFinished) {
                FixedUpdateInternal();
            }
        }

        /// <inheritdoc/>
        void ISituation.PreClose(TransitionHandle<Situation> handle) {
            if (CurrentState <= State.SetupFinished) {
                return;
            }

            PreCloseInternal(handle);
        }

        /// <inheritdoc/>
        IEnumerator ISituation.CloseRoutine(TransitionHandle<Situation> handle) {
            yield return CloseRoutineInternal(handle, _animationScope);
            _animationScope.Clear();
        }

        /// <inheritdoc/>
        void ISituation.PostClose(TransitionHandle<Situation> handle) {
            if (CurrentState <= State.SetupFinished) {
                return;
            }

            CurrentState = State.SetupFinished;
            PostCloseInternal(handle);
            _openScope.Clear();
        }

        /// <inheritdoc/>
        void ISituation.Deactivate(TransitionHandle<Situation> handle) {
            if (!IsActive) {
                return;
            }
            
            // 非アクティブ時にはフォーカスは外す
            ((ISituation)this).SetFocus(false);

            IsActive = false;
            DeactivateInternal(handle);
            _activeScope.Clear();
        }

        /// <inheritdoc/>
        void ISituation.Cleanup(TransitionHandle<Situation> handle) {
            if (CurrentState <= State.Loaded) {
                return;
            }

            CurrentState = State.Loaded;
            CleanupInternal(handle);
            _setupScope.Clear();
        }

        /// <inheritdoc/>
        void ISituation.Unload(TransitionHandle<Situation> handle) {
            if (CurrentState <= State.Standby) {
                return;
            }

            if (PreLoadState != PreLoadState.None) {
                return;
            }

            CurrentState = State.Standby;
            UnloadInternal(handle);
            _loadScope.Clear();
            ServiceContainer.Dispose();
        }

        /// <inheritdoc/>
        void ISituation.Release(SituationContainer container) {
            if (_container != null && _container != container) {
                Debug.LogError("Invalid release parent.");
                return;
            }

            if (CurrentState <= State.Invalid) {
                return;
            }

            var info = new SituationContainer.TransitionInfo {
                Direction = TransitionDirection.Forward,
                State = TransitionState.Canceled,
                EndStep = TransitionStep.Complete,
                PrevSituations = new[] { this },
                NextSituations = Array.Empty<Situation>(),
            };
            var handle = new TransitionHandle<Situation>(info);

            var situation = (ISituation)this;
            if (CurrentState >= State.Opening) {
                situation.PreClose(handle);
                situation.PostClose(handle);
            }

            situation.Deactivate(handle);

            if (CurrentState >= State.SetupFinished) {
                situation.Cleanup(handle);
            }

            // PreLoadはここで終わり
            if (PreLoadState != PreLoadState.None) {
                return;
            }

            if (CurrentState >= State.Loaded) {
                situation.Unload(handle);
            }

            CurrentState = State.Invalid;
            ReleaseInternal(container);
        }

        /// <inheritdoc/>
        IEnumerator ISituation.PreLoadRoutine() {
            if (PreLoadState != PreLoadState.None) {
                yield break;
            }

            var situation = (ISituation)this;
            if (!situation.CanPreLoad) {
                yield break;
            }

            PreLoadState = PreLoadState.PreLoading;
            yield return situation.LoadRoutine(TransitionHandle<Situation>.Empty, true);
            PreLoadState = PreLoadState.PreLoaded;
        }

        /// <inheritdoc/>
        void ISituation.UnPreLoad() {
            if (PreLoadState == PreLoadState.None) {
                return;
            }

            var situation = (ISituation)this;
            if (!situation.CanPreLoad) {
                return;
            }

            PreLoadState = PreLoadState.None;

            // 稼働中ならUnloadは呼ばない
            if (CurrentState >= State.SetupFinished) {
                return;
            }

            situation.Unload(TransitionHandle<Situation>.Empty);
        }

        /// <inheritdoc/>
        void ISituation.SetFocus(bool focus) {
            if (focus == IsFocused) {
                return;
            }

            IsFocused = focus;
            if (IsFocused) {
                FocusedInternal(_focusScope);
            }
            else {
                UnfocusedInternal();
                _focusScope.Clear();
            }
        }

        /// <summary>
        /// 遷移用のデフォルトTransition取得
        /// </summary>
        public virtual ITransition GetDefaultNextTransition() {
            return new OutInTransition();
        }

        /// <summary>
        /// 親要素として適切かチェックする
        /// </summary>
        protected virtual bool CheckParent(ISituation parent) {
            return true;
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        /// <param name="container">登録されたコンテナ</param>
        protected virtual void StandbyInternal(SituationContainer container) {
        }

        /// <summary>
        /// 読み込み処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(LoadRoutine～Unloadまで)</param>
        protected virtual IEnumerator LoadRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// 初期化処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(Setup～Cleanupまで)</param>
        protected virtual IEnumerator SetupRoutineInternal(TransitionHandle<Situation> handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// 開く直前処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(PreOpen～PostCloseまで)</param>
        protected virtual void PreOpenInternal(TransitionHandle<Situation> handle, IScope scope) {
        }

        /// <summary>
        /// 開く処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="animationScope">アニメーションキャンセル用スコープ(OpenRoutine中)</param>
        protected virtual IEnumerator OpenRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield break;
        }

        /// <summary>
        /// 開く直後処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(PreOpen～PostCloseまで)</param>
        protected virtual void PostOpenInternal(TransitionHandle<Situation> handle, IScope scope) {
        }

        /// <summary>
        /// アクティブ処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(Active～Deactiveまで)</param>
        protected virtual void ActivateInternal(TransitionHandle<Situation> handle, IScope scope) {
        }

        /// <summary>
        /// 更新処理(内部用)
        /// </summary>
        protected virtual void UpdateInternal() {
        }

        /// <summary>
        /// 後更新処理(内部用)
        /// </summary>
        protected virtual void LateUpdateInternal() {
        }

        /// <summary>
        /// 物理更新処理(内部用)
        /// </summary>
        protected virtual void FixedUpdateInternal() {
        }

        /// <summary>
        /// 非アクティブ処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void DeactivateInternal(TransitionHandle<Situation> handle) {
        }

        /// <summary>
        /// 閉じる直前処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PreCloseInternal(TransitionHandle<Situation> handle) {
        }

        /// <summary>
        /// 閉じる処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="animationScope">アニメーションキャンセル用スコープ(OpenRoutine中)</param>
        protected virtual IEnumerator CloseRoutineInternal(TransitionHandle<Situation> handle, IScope animationScope) {
            yield break;
        }

        /// <summary>
        /// 閉じる直後処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PostCloseInternal(TransitionHandle<Situation> handle) {
        }

        /// <summary>
        /// 初期化処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void CleanupInternal(TransitionHandle<Situation> handle) {
        }

        /// <summary>
        /// 解放処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void UnloadInternal(TransitionHandle<Situation> handle) {
        }

        /// <summary>
        /// 登録解除処理
        /// </summary>
        /// <param name="parent">登録されていたContainer</param>
        protected virtual void ReleaseInternal(SituationContainer parent) {
        }

        /// <summary>
        /// Active以外も実行される更新処理(内部用)
        /// </summary>
        protected virtual void SystemUpdateInternal() {
        }

        /// <summary>
        /// Active以外も実行される後更新処理(内部用)
        /// </summary>
        protected virtual void SystemLateUpdateInternal() {
        }

        /// <summary>
        /// Active以外も実行される物理更新処理(内部用)
        /// </summary>
        protected virtual void SystemFixedUpdateInternal() {
        }

        /// <summary>
        /// フォーカスを得た時の処理
        /// </summary>
        protected virtual void FocusedInternal(IScope scope) {
        }

        /// <summary>
        /// フォーカスを失った時の処理
        /// </summary>
        protected virtual void UnfocusedInternal() {
        }

        /// <summary>
        /// 親の設定
        /// </summary>
        public void SetParent(Situation parent) {
            if (parent == _parent) {
                return;
            }

            // 接続親が適切かチェック
            if (parent != null && !CheckParent(parent)) {
                Debug.LogError($"Invalid parent. {GetType().Name} in {parent.GetType().Name}");
                return;
            }

            // 現在の親Containerがあったら抜ける
            if (_parent != null) {
                if (_container != null) {
                    ((ISituation)this).Release(_container);
                }

                _parent._children.Remove(this);
                _parent = null;
            }

            // 親の設定
            _parent = parent;

            // コンテナに登録
            if (_parent != null) {
                _parent._children.Add(this);
                if (_parent._container != null) {
                    ((ISituation)this).Standby(_parent._container);
                }
            }
        }

        /// <summary>
        /// 事前読み込み
        /// </summary>
        public AsyncOperationHandle PreLoadAsync() {
            var asyncOp = new AsyncOperator();
            if (_container == null) {
                asyncOp.Aborted(new Exception("Not found container."));
                return asyncOp.GetHandle();
            }

            return _container.PreLoadAsync(GetType());
        }

        /// <summary>
        /// 事前読み込み解除
        /// </summary>
        public void UnPreLoad() {
            if (_container == null) {
                return;
            }

            _container.UnPreLoad(GetType());
        }
    }
}