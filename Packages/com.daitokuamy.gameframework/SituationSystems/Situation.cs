using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション
    /// </summary>
    public abstract class Situation : ISituation {
        // 状態
        public enum State {
            Invalid = -1,
            Standby, // 待機状態
            Loading, // 読み込み中
            Loaded, // 読み込み済
            SetupFinished, // 初期化済
            Active, // アクティブ中
        }

        // 読み込みスコープ
        private DisposableScope _loadScope;
        // 初期化スコープ
        private DisposableScope _setupScope;
        // アニメーションスコープ
        private DisposableScope _animationScope;
        // アクティブスコープ
        private DisposableScope _activeScope;

        // 子SituationContainer
        private Dictionary<int, SituationContainer> _childContainers = new();

        /// <summary>親のSituation</summary>
        public Situation Parent => ParentContainer?.Owner;
        /// <summary>登録されているContainer</summary>
        public SituationContainer ParentContainer { get; private set; }
        /// <summary>子階層を登録するためのContainer</summary>
        public SituationContainer ChildContainer { get; private set; }
        /// <summary>インスタンス管理用</summary>
        public ServiceContainer ServiceContainer { get; private set; }
        /// <summary>現在状態</summary>
        public State CurrentState { get; private set; } = State.Invalid;
        /// <summary>コンテナ登録されているか</summary>
        public bool PreRegistered { get; private set; } = false;
        /// <summary>プリロード状態</summary>
        public PreLoadState PreLoadState { get; private set; } = PreLoadState.None;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Situation() {
        }

        /// <summary>
        /// スタンバイ処理
        /// </summary>
        void ISituation.Standby(SituationContainer container) {
            if (ParentContainer != null && container != ParentContainer) {
                Debug.LogError("Already exists container.");
                return;
            }

            if (CurrentState >= State.Standby) {
                return;
            }

            CurrentState = State.Standby;
            ParentContainer = container;
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
        /// 開く処理
        /// </summary>
        IEnumerator ISituation.OpenRoutine(TransitionHandle handle) {
            _animationScope = new DisposableScope();
            yield return OpenRoutineInternal(handle, _animationScope);
            _animationScope.Dispose();
            _animationScope = null;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        void ISituation.Activate(TransitionHandle handle) {
            if (CurrentState >= State.Active) {
                return;
            }

            _activeScope = new DisposableScope();
            ActivateInternal(handle, _activeScope);
            CurrentState = State.Active;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ISituation.Update() {
            UpdateInternal();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        void ISituation.LateUpdate() {
            LateUpdateInternal();
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        void ISituation.Deactivate(TransitionHandle handle) {
            if (CurrentState <= State.SetupFinished) {
                return;
            }

            CurrentState = State.SetupFinished;
            DeactivateInternal(handle);
            _activeScope.Dispose();
            _activeScope = null;
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
            if (ParentContainer != null && container != ParentContainer) {
                Debug.LogError("Invalid release parent.");
                return;
            }

            if (CurrentState <= State.Invalid) {
                return;
            }

            var info = new SituationContainer.TransitionInfo {
                container = container,
                prev = this,
                next = null,
                back = false,
                state = TransitionState.Canceled
            };
            var handle = new TransitionHandle(info);

            var situation = (ISituation)this;
            if (CurrentState == State.Active) {
                situation.Deactivate(handle);
            }

            if (CurrentState == State.SetupFinished) {
                situation.Cleanup(handle);
            }

            // PreLoadはここで終わり
            if (PreLoadState != PreLoadState.None) {
                return;
            }

            if (CurrentState == State.Loaded) {
                situation.Unload(handle);
            }
            
            // PreRegisterはここで終わり
            if (PreRegistered) {
                return;
            }

            ReleaseInternal(container);

            ParentContainer = null;
            CurrentState = State.Invalid;
        }

        /// <summary>
        /// コンテナの事前登録
        /// </summary>
        /// <param name="container">登録するコンテナ</param>
        void ISituation.PreRegister(SituationContainer container) {
            if (PreRegistered) {
                Debug.LogWarning($"Already pre register situation. [{GetType().Name}]");
                return;
            }

            var situation = (ISituation)this;
            PreRegistered = true;
            
            situation.Standby(container);
        }

        /// <summary>
        /// コンテナの事前登録解除
        /// </summary>
        /// <param name="container">登録するコンテナ</param>
        void ISituation.PreUnregister(SituationContainer container) {
            if (!PreRegistered) {
                return;
            }

            var situation = (ISituation)this;
            PreRegistered = false;

            // 稼働中ならReleaseは呼ばない
            if (CurrentState >= State.Loaded) {
                return;
            }
            
            situation.Release(container);
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
        /// 更新処理
        /// </summary>
        void ISituation.SystemUpdate() {
            SystemUpdateInternal();

            foreach (var pair in _childContainers) {
                pair.Value.Update();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        void ISituation.SystemLateUpdate() {
            SystemLateUpdateInternal();

            foreach (var pair in _childContainers) {
                pair.Value.LateUpdate();
            }
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
        /// 開く処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        /// <param name="animationScope">アニメーションキャンセル用スコープ(OpenRoutine中)</param>
        protected virtual IEnumerator OpenRoutineInternal(TransitionHandle handle, IScope animationScope) {
            yield break;
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
        /// 非アクティブ処理(内部用)
        /// </summary>
        /// <param name="handle">遷移ハンドル</param>
        protected virtual void DeactivateInternal(TransitionHandle handle) {
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
        /// 更新処理
        /// </summary>
        public void Update() {
            if (CurrentState == State.Invalid) {
                return;
            }

            // Systemの更新
            ((ISituation)this).SystemUpdate();

            // UpdateはActive中のみ
            if (CurrentState == State.Active) {
                ((ISituation)this).Update();
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        public void LateUpdate() {
            if (CurrentState == State.Invalid) {
                return;
            }

            // Systemの更新
            ((ISituation)this).SystemLateUpdate();

            // LateUpdateはActive中のみ
            if (CurrentState == State.Active) {
                ((ISituation)this).LateUpdate();
            }
        }

        /// <summary>
        /// 子Situationの追加
        /// </summary>
        /// <param name="containerIndex">ContainerのIndex</param>
        /// <param name="situation">登録するSituation</param>
        public void RegisterChild(int containerIndex, Situation situation) {
            var container = GetChildContainer(containerIndex);
            if (container == null) {
                Debug.LogWarning($"Not found child container[{containerIndex}].");
                return;
            }
            
            container.PreRegister(situation);
        }

        /// <summary>
        /// 子Situationの登録
        /// </summary>
        /// <param name="situation">登録するSituation</param>
        public void RegisterChild(Situation situation) {
            RegisterChild(0, situation);
        }

        /// <summary>
        /// 子Situationの登録解除
        /// </summary>
        /// <param name="situation">登録解除するSituation</param>
        public void UnRegisterChild(Situation situation) {
            foreach (var container in _childContainers.Values) {
                if (container.ContainsPreRegister(situation)) {
                    container.PreUnregister(situation);
                    break;
                }
            }
        }

        /// <summary>
        /// 子を登録するためのContainerの取得
        /// </summary>
        public SituationContainer GetChildContainer(int index) {
            if (_childContainers.TryGetValue(index, out var container)) {
                return container;
            }

            return null;
        }

        /// <summary>
        /// 子を登録するためのContainerを生成
        /// </summary>
        public T CreateChildContainer<T>(int index, bool useStack = true)
            where T : SituationContainer, new() {
            if (_childContainers.TryGetValue(index, out var container)) {
                container.Dispose();
            }

            container = new T();
            container.InitializeInternal(this, useStack);
            _childContainers.Add(index, container);
            return (T)container;
        }

        /// <summary>
        /// 子を登録するためのContainerを生成
        /// </summary>
        public SituationContainer CreateChildContainer(int index, bool useStack = true) {
            return CreateChildContainer<SituationContainer>(index, useStack);
        }

        /// <summary>
        /// 子のSituationContainerの削除
        /// </summary>
        public void DestroyChildContainer(int index) {
            if (_childContainers.TryGetValue(index, out var container)) {
                container.Dispose();
                _childContainers.Remove(index);
            }
        }
    }
}