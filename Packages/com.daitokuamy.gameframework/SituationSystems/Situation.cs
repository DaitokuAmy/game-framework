using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション
    /// </summary>
    public abstract class Situation : ISituation, ISituationContainerProvider, IDisposable {
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

        // 子Situation
        private readonly List<Situation> _children = new();
        // 読み込みスコープ
        private DisposableScope _loadScope;
        // 初期化スコープ
        private DisposableScope _setupScope;
        // アクティブスコープ
        private DisposableScope _activeScope;
        // オープンスコープ
        private DisposableScope _openScope;
        // アニメーションスコープ
        private DisposableScope _animationScope;
        // 登録されているコンテナ
        private SituationContainer _container;

        /// <summary>インターフェース用の子コンテナ返却プロパティ</summary>
        SituationContainer ISituationContainerProvider.Container => _container;

        /// <summary>親のSituation</summary>
        public Situation Parent { get; private set; }
        /// <summary>子Situationリスト</summary>
        public IReadOnlyList<Situation> Children => _children;
        /// <summary>インスタンス管理用</summary>
        public ServiceContainer ServiceContainer { get; private set; }
        /// <summary>現在状態</summary>
        public State CurrentState { get; private set; } = State.Invalid;
        /// <summary>アクティブ状態か</summary>
        public bool IsActive { get; private set; }
        /// <summary>プリロード状態</summary>
        public PreLoadState PreLoadState { get; private set; } = PreLoadState.None;
        /// <summary>登録されているFlow</summary>
        protected SituationFlow SituationFlow { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Situation() {
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
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
            StandbyInternal(Parent);
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        IEnumerator ISituation.LoadRoutine(TransitionHandle handle, bool preload) {
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

            _loadScope = new DisposableScope();
            ServiceContainer = new ServiceContainer(Parent?.ServiceContainer ?? Services.Instance);
            CurrentState = State.Loading;
            yield return LoadRoutineInternal(handle, _loadScope);
            CurrentState = State.Loaded;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        IEnumerator ISituation.SetupRoutine(TransitionHandle handle) {
            if (CurrentState >= State.SetupFinished) {
                yield break;
            }

            _setupScope = new DisposableScope();
            yield return SetupRoutineInternal(handle, _setupScope);
            CurrentState = State.SetupFinished;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        void ISituation.Activate(TransitionHandle handle) {
            if (IsActive) {
                return;
            }

            IsActive = true;
            _activeScope = new DisposableScope();
            ActivateInternal(handle, _activeScope);
        }

        /// <summary>
        /// 開く直前の処理
        /// </summary>
        void ISituation.PreOpen(TransitionHandle handle) {
            if (CurrentState >= State.Opening) {
                return;
            }

            _openScope = new DisposableScope();
            PreOpenInternal(handle, _openScope);
            CurrentState = State.Opening;
        }

        /// <summary>
        /// 開く処理
        /// </summary>
        IEnumerator ISituation.OpenRoutine(TransitionHandle handle) {
            _animationScope = new DisposableScope();
            yield return OpenRoutineInternal(handle, _animationScope);
            _animationScope.Dispose();
            _animationScope = null;
        }

        /// <summary>
        /// 開く直後の処理
        /// </summary>
        void ISituation.PostOpen(TransitionHandle handle) {
            PostOpenInternal(handle, _openScope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
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

            // 子の更新
            foreach (ISituation child in _children) {
                child.Update();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
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

            // 子の更新
            foreach (ISituation child in _children) {
                child.LateUpdate();
            }
        }

        /// <summary>
        /// 物理更新処理
        /// </summary>
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

            // 子の更新
            foreach (ISituation child in _children) {
                child.FixedUpdate();
            }
        }

        /// <summary>
        /// 閉じる直前の処理
        /// </summary>
        void ISituation.PreClose(TransitionHandle handle) {
            if (CurrentState <= State.SetupFinished) {
                return;
            }

            PreCloseInternal(handle);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        IEnumerator ISituation.CloseRoutine(TransitionHandle handle) {
            _animationScope = new DisposableScope();
            yield return CloseRoutineInternal(handle, _animationScope);
            _animationScope.Dispose();
            _animationScope = null;
        }

        /// <summary>
        /// 閉じる直後の処理
        /// </summary>
        void ISituation.PostClose(TransitionHandle handle) {
            if (CurrentState <= State.SetupFinished) {
                return;
            }

            CurrentState = State.SetupFinished;
            PostCloseInternal(handle);
            _openScope.Dispose();
            _openScope = null;
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        void ISituation.Deactivate(TransitionHandle handle) {
            if (!IsActive) {
                return;
            }

            IsActive = false;
            DeactivateInternal(handle);
            _activeScope.Dispose();
            _activeScope = null;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        void ISituation.Cleanup(TransitionHandle handle) {
            if (CurrentState <= State.Loaded) {
                return;
            }

            CurrentState = State.Loaded;
            CleanupInternal(handle);
            _setupScope.Dispose();
            _setupScope = null;
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        void ISituation.Unload(TransitionHandle handle) {
            if (CurrentState <= State.Standby) {
                return;
            }

            if (PreLoadState != PreLoadState.None) {
                return;
            }

            CurrentState = State.Standby;
            UnloadInternal(handle);
            _loadScope.Dispose();
            ServiceContainer.Dispose();
            _loadScope = null;
        }

        /// <summary>
        /// 登録解除処理
        /// </summary>
        void ISituation.Release(SituationContainer container) {
            if (_container != null && _container != container) {
                Debug.LogError("Invalid release parent.");
                return;
            }

            if (CurrentState <= State.Invalid) {
                return;
            }

            var info = new SituationContainer.TransitionInfo {
                Container = container,
                Prev = this,
                Next = null,
                Back = false,
                State = TransitionState.Canceled
            };
            var handle = new TransitionHandle(info);

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

        /// <summary>
        /// プリロード処理
        /// </summary>
        IEnumerator ISituation.PreLoadRoutine() {
            if (PreLoadState != PreLoadState.None) {
                yield break;
            }

            PreLoadState = PreLoadState.PreLoading;
            var situation = (ISituation)this;
            yield return situation.LoadRoutine(new TransitionHandle(), true);
            PreLoadState = PreLoadState.PreLoaded;
        }

        /// <summary>
        /// プリロード解除処理
        /// </summary>
        void ISituation.UnPreLoad() {
            if (PreLoadState == PreLoadState.None) {
                return;
            }

            var situation = (ISituation)this;
            PreLoadState = PreLoadState.None;

            // 稼働中ならUnloadは呼ばない
            if (CurrentState >= State.SetupFinished) {
                return;
            }

            situation.Unload(new TransitionHandle());
        }

        /// <summary>
        /// 遷移用のデフォルトTransition取得
        /// </summary>
        public virtual ITransition GetDefaultNextTransition() {
            return new OutInTransition();
        }

        /// <summary>
        /// 遷移可能かチェック
        /// </summary>
        /// <param name="nextTransition">遷移するの子シチュエーション</param>
        /// <param name="transition">遷移処理</param>
        /// <returns>遷移可能か</returns>
        public virtual bool CheckNextTransition(Situation nextTransition, ITransition transition) {
            return true;
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        /// <param name="parent">親シチュエーション</param>
        protected virtual void StandbyInternal(Situation parent) {
        }

        /// <summary>
        /// 読み込み処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(LoadRoutine～Unloadまで)</param>
        protected virtual IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// 初期化処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(Setup～Cleanupまで)</param>
        protected virtual IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
            yield break;
        }

        /// <summary>
        /// 開く直前処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(PreOpen～PostCloseまで)</param>
        protected virtual void PreOpenInternal(TransitionHandle handle, IScope scope) {
        }

        /// <summary>
        /// 開く処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="animationScope">アニメーションキャンセル用スコープ(OpenRoutine中)</param>
        protected virtual IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield break;
        }

        /// <summary>
        /// 開く直後処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(PreOpen～PostCloseまで)</param>
        protected virtual void PostOpenInternal(TransitionHandle handle, IScope scope) {
        }

        /// <summary>
        /// アクティブ処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="scope">スコープ(Active～Deactiveまで)</param>
        protected virtual void ActivateInternal(TransitionHandle handle, IScope scope) {
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
        protected virtual void DeactivateInternal(TransitionHandle handle) {
        }

        /// <summary>
        /// 閉じる直前処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PreCloseInternal(TransitionHandle handle) {
        }

        /// <summary>
        /// 閉じる処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="animationScope">アニメーションキャンセル用スコープ(OpenRoutine中)</param>
        protected virtual IEnumerator CloseRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield break;
        }

        /// <summary>
        /// 閉じる直後処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void PostCloseInternal(TransitionHandle handle) {
        }

        /// <summary>
        /// 初期化処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void CleanupInternal(TransitionHandle handle) {
        }

        /// <summary>
        /// 解放処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void UnloadInternal(TransitionHandle handle) {
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
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_container != null) {
                if (PreLoadState != PreLoadState.None) {
                    _container.UnPreLoad(this);
                }
            }
        }

        /// <summary>
        /// 親の設定
        /// </summary>
        public void SetParent(Situation parent) {
            // 現在の親Containerがあったら抜ける
            if (Parent != null) {
                if (_container != null) {
                    ((ISituation)this).Release(_container);
                }

                Parent._children.Remove(this);
            }

            if (parent == null) {
                return;
            }

            // コンテナに登録
            parent._children.Add(this);
            if (parent._container != null) {
                ((ISituation)this).Standby(parent._container);
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

            return _container.PreLoadAsync(this);
        }

        /// <summary>
        /// 事前読み込み解除
        /// </summary>
        public void UnPreLoad() {
            if (_container == null) {
                return;
            }

            _container.UnPreLoad(this);
        }
    }
}