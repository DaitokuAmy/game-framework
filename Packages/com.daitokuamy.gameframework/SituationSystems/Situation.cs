using System.Collections;
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

        // 親のSituation
        public Situation Parent => ParentContainer?.Owner;
        // 登録されているContainer
        public SituationContainer ParentContainer { get; private set; }
        // インスタンス管理用
        public ServiceContainer ServiceContainer { get; private set; }
        // 現在状態
        public State CurrentState { get; private set; } = State.Invalid;
        // プリロードされているか
        public bool PreLoaded { get; private set; } = false;

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
        IEnumerator ISituation.LoadRoutine(TransitionHandle handle) {
            if (CurrentState >= State.Loaded) {
                yield break;
            }

            // PreLoadしている場合、Loadedになるのを待つ
            if (PreLoaded) {
                while (CurrentState < State.Loaded) {
                    yield return null;
                }

                yield break;
            }

            _loadScope = new DisposableScope();
            ServiceContainer = new ServiceContainer(Parent?.ServiceContainer ?? Services.Instance);
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
            if (PreLoaded) {
                return;
            }

            if (CurrentState == State.Loaded) {
                situation.Unload(handle);
            }

            ReleaseInternal(container);

            ParentContainer = null;
            CurrentState = State.Invalid;
        }

        /// <summary>
        /// プリロード処理
        /// </summary>
        IEnumerator ISituation.PreLoadRoutine() {
            if (PreLoaded) {
                yield break;
            }

            var situation = (ISituation)this;
            PreLoaded = true;

            yield return situation.LoadRoutine(new TransitionHandle());
        }

        /// <summary>
        /// プリロード解除処理
        /// </summary>
        void ISituation.UnPreLoad() {
            if (!PreLoaded) {
                return;
            }

            var situation = (ISituation)this;
            PreLoaded = false;

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
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        void ISituation.SystemLateUpdate() {
            SystemLateUpdateInternal();
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
    }
}