using System;
using System.Collections;
using UnityEngine;

namespace GameFramework.BootSystem {
    /// <summary>
    /// システムのブートをハンドリングするためのクラス
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class BootManager : MonoBehaviour {
        /// <summary>
        /// 状態
        /// </summary>
        private enum State {
            /// <summary>無効値</summary>
            Invalid = -1,
            /// <summary>初期化中</summary>
            Starting,
            /// <summary>アクティブ</summary>
            Active,
            /// <summary>リブート中</summary>
            Rebooting,
            /// <summary>廃棄済み</summary>
            Destroyed,
        }

        private static BootManager s_instance;

        [SerializeField, Tooltip("実行処理を記述するメインシステム")]
        private MainSystemBase _mainSystem;

        private State _currentState = State.Invalid;

        /// <summary>存在するか</summary>
        public static bool IsExists => s_instance != null;
        /// <summary>アクティブ状態か</summary>
        public static bool IsActive => (s_instance?._currentState ?? State.Invalid) == State.Active;

        /// <summary>
        /// リブート処理
        /// </summary>
        /// <param name="args">リブート時に渡す引数</param>
        public static void Reboot(params object[] args) {
            if (s_instance == null) {
                throw new InvalidOperationException("boot manager is not found.");
            }

            s_instance.RebootInternal(args);
        }

        /// <summary>
        /// リブート処理(内部用)
        /// </summary>
        private void RebootInternal(params object[] args) {
            if (_currentState != State.Active) {
                throw new InvalidOperationException($"Invalid boot manager state. {_currentState}");
            }

            // リブート状態にしてコルーチン実行
            StartCoroutine(RebootRoutine(args));
        }

        /// <summary>
        /// 初期化処理コルーチン
        /// </summary>
        private IEnumerator StartRoutine(object[] args) {
            _currentState = State.Starting;

            try {
                if (_mainSystem != null) {
                    yield return _mainSystem.StartRoutine(args);
                }
                else {
                    Debug.LogWarning("MainSystem is null.");
                }
            }
            finally {
                if (s_instance == this) {
                    _currentState = State.Active;
                }
            }
        }

        /// <summary>
        /// リブート処理コルーチン
        /// </summary>
        /// <param name="args">リブート時に渡された引数</param>
        private IEnumerator RebootRoutine(object[] args) {
            _currentState = State.Rebooting;

            try {
                if (_mainSystem != null) {
                    yield return _mainSystem.RebootRoutine(args);
                }
                else {
                    Debug.LogWarning("MainSystem is null.");
                }
            }
            finally {
                if (s_instance == this) {
                    _currentState = State.Active;
                }
            }
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            if (s_instance != null) {
                DestroyImmediate(this);
                Debug.LogWarning($"Already exists BootManager.");
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            if (s_instance != this) {
                return;
            }

            _currentState = State.Destroyed;
            s_instance = null;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        private IEnumerator Start() {
            if (s_instance != this) {
                yield break;
            }

            // Starterから引数を取得
            var starter = MainSystemStarter.Current;
            var arguments = starter != null ? starter.GetArguments() ?? Array.Empty<object>() : Array.Empty<object>();

            // 開始処理
            yield return StartRoutine(arguments);

            // Starterを削除
            if (starter != null) {
                Destroy(starter.gameObject);
            }
        }
    }
}
