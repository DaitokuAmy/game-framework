using System;
using System.Collections;
using UnityEngine;

namespace GameFramework {
    /// <summary>
    /// アプリケーションのメインシステム
    /// </summary>
    public abstract class MainSystem : MonoBehaviour {
        // 状態
        private enum State {
            // 無効値
            Invalid = -1,
            // 初期化中
            Starting,
            // 有効状態
            Active,
            // リブート中
            Rebooting,
            // 廃棄済み
            Destroyed,
        }

        // 現在の状態
        private State _currentState = State.Invalid;

        // 自身がカレントシステムか
        public bool IsCurrent => Current == this;
        // アクティブ状態か
        public bool IsActive => _currentState == State.Active;

        // メインシステムが存在するか
        public static bool Exists => Current != null;
        // 現在のメインシステム
        private static MainSystem Current { get; set; }

        /// <summary>
        /// リブート処理
        /// </summary>
        /// <param name="args">リブート時に渡す引数</param>
        public void Reboot(params object[] args) {
            if (_currentState != State.Active) {
                Debug.LogError($"Invalid main system state. {_currentState}");
                return;
            }

            // リブート状態にしてコルーチン実行
            StartCoroutine(RebootRoutine(args));
        }

        /// <summary>
        /// リブート処理記述用コルーチン
        /// </summary>
        /// <param name="args">リブート時に渡された引数</param>
        protected abstract IEnumerator RebootRoutineInternal(object[] args);

        /// <summary>
        /// 開始処理記述用コルーチン
        /// </summary>
        /// <param name="args">MainSystemStarterから渡された引数</param>
        protected abstract IEnumerator StartRoutineInternal(object[] args);

        /// <summary>
        /// メイン更新処理
        /// </summary>
        protected abstract void UpdateInternal();

        /// <summary>
        /// メイン後更新処理
        /// </summary>
        protected abstract void LateUpdateInternal();

        /// <summary>
        /// メイン固定更新処理
        /// </summary>
        protected abstract void FixedUpdateInternal();

        /// <summary>
        /// メイン廃棄時処理
        /// </summary>
        protected abstract void OnDestroyInternal();

        /// <summary>
        /// 初期化処理コルーチン
        /// </summary>
        private IEnumerator StartRoutine(object[] args) {
            _currentState = State.Starting;
            DontDestroyOnLoad(gameObject);
            yield return StartRoutineInternal(args);
            _currentState = State.Active;
        }

        /// <summary>
        /// リブート処理コルーチン
        /// </summary>
        /// <param name="args">リブート時に渡された引数</param>
        private IEnumerator RebootRoutine(object[] args) {
            _currentState = State.Rebooting;
            yield return RebootRoutineInternal(args);
            _currentState = State.Active;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected virtual void Awake() {
            if (Current != null) {
                DestroyImmediate(this);
                Debug.LogWarning($"Already exists MainSystem. {Current.GetType().Name}");
                return;
            }

            Current = this;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void OnDestroy() {
            if (!IsCurrent) {
                return;
            }

            OnDestroyInternal();
            _currentState = State.Destroyed;
            Current = null;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        private IEnumerator Start() {
            if (!IsCurrent) {
                yield break;
            }

            // Starterから引数を取得
            var starter = MainSystemStarter.Current;
            var arguments = starter != null ? starter.GetArguments() : Array.Empty<object>();

            // 開始処理
            yield return StartRoutine(arguments);

            // Starterを削除
            if (starter != null) {
                Destroy(starter.gameObject);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
            if (!IsCurrent) {
                return;
            }

            UpdateInternal();
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            if (!IsCurrent) {
                return;
            }

            LateUpdateInternal();
        }

        /// <summary>
        /// 固定更新処理
        /// </summary>
        private void FixedUpdate() {
            if (!IsCurrent) {
                return;
            }

            FixedUpdateInternal();
        }
    }

    /// <summary>
    /// Generic派生用のMainSystem
    /// </summary>
    public abstract class MainSystem<T> : MainSystem
        where T : MainSystem<T> {
        public static T Instance { get; private set; }

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected sealed override void Awake() {
            base.Awake();

            if (!IsCurrent) {
                return;
            }

            Instance = (T)this;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected sealed override void OnDestroy() {
            base.OnDestroy();

            if (this == Instance) {
                Instance = null;
            }
        }
    }
}