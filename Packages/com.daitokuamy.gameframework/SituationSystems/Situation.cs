using System;
using System.Collections;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション
    /// </summary>
    public abstract class Situation : ISituation, INodeSituation, ISituationContainerProvider, IDisposable {
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

        /// <summary>インターフェース用の子コンテナ返却プロパティ</summary>
        SituationContainer ISituationContainerProvider.Container => ChildContainer;

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
        /// <summary>登録されているFlow</summary>
        protected SituationFlow SituationFlow { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Situation(bool useStack = false) {
            ChildContainer = new SituationContainer(this, useStack);
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
            if (CurrentState == State.Invalid) {
                return;
            }

            // Systemの更新
            SystemUpdateInternal();

            // UpdateはActive中のみ
            if (CurrentState == State.Active) {
                UpdateInternal();
            }

            // 子コンテナの更新
            ChildContainer.Update();
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
            if (CurrentState == State.Active) {
                LateUpdateInternal();
            }

            // 子コンテナの更新
            ChildContainer.LateUpdate();
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
            if (CurrentState == State.Active) {
                FixedUpdateInternal();
            }

            // 子コンテナの更新
            ChildContainer.FixedUpdate();
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
        /// Tree登録通知
        /// </summary>
        void INodeSituation.OnRegisterFlow(SituationFlow flow) {
            SituationFlow = flow;
        }

        /// <summary>
        /// Tree登録解除通知
        /// </summary>
        void INodeSituation.OnUnregisterFlow(SituationFlow flow) {
            if (flow == SituationFlow) {
                SituationFlow = null;
            }
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
        /// Active以外も実行される物理更新処理(内部用)
        /// </summary>
        protected virtual void SystemFixedUpdateInternal() {
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (ParentContainer != null) {
                if (PreLoadState != PreLoadState.None) {
                    ParentContainer.UnPreLoad(this);
                }
                
                if (PreRegistered) {
                    ParentContainer.UnPreRegister(this);
                }

                ParentContainer?.ForceRemove(this);
            }
        }

        /// <summary>
        /// 親の設定
        /// </summary>
        /// <param name="provider">Situation or SituationRunner</param>
        public void SetParent(ISituationContainerProvider provider) {
            // 現在の親Containerがあったら抜ける
            if (ParentContainer != null) {
                if (PreRegistered) {
                    ParentContainer.UnPreRegister(this);
                }

                ParentContainer.Remove(this);
            }
            
            if (provider == null) {
                return;
            }

            // コンテナに登録
            provider.Container.PreRegister(this);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        protected IProcess Transition<T>(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where T : Situation {
            if (SituationFlow == null) {
                return ParentContainer.Transition<T>(onSetup, overrideTransition, effects);
            }
            
            return SituationFlow.Transition<T>(onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="type">遷移先を表すSituatonのType</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public IProcess Transition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            if (SituationFlow == null) {
                return ParentContainer.Transition(type, onSetup, overrideTransition, effects);
            }
            
            return SituationFlow.Transition(type, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public IProcess Transition(SituationFlowNode nextNode, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            if (SituationFlow == null) {
                return new TransitionHandle(new Exception("Not found situation flow."));
            }

            return SituationFlow.Transition(nextNode, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        protected IProcess Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            if (SituationFlow == null) {
                return ParentContainer.Back(overrideTransition, effects);
            }
            
            return SituationFlow.Back(overrideTransition, effects);
        }

        /// <summary>
        /// 遷移可能かチェック
        /// </summary>
        /// <param name="includeFallback">Fallback対象の型を含めるか</param>
        /// <typeparam name="T">チェックする型</typeparam>
        /// <returns>遷移可能か</returns>
        protected bool CheckTransition<T>(bool includeFallback = true)
            where T : Situation {
            if (SituationFlow == null) {
                return ParentContainer.ContainsPreRegister<T>();
            }
            
            return SituationFlow.CheckTransition<T>(includeFallback);
        }
    }
}