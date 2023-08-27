using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.Core {
    /// <summary>
    /// MainSystem開始用クラス
    /// </summary>
    public abstract class MainSystemStarter : MonoBehaviour {
        [SerializeField, Tooltip("Bootに使用するシーン名")]
        private string _bootSceneName = "";

        // 使用しているStarter
        public static MainSystemStarter Current { get; private set; }

        /// <summary>
        /// MainSystemに渡す引数
        /// </summary>
        public abstract object[] GetArguments();

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            if (Current != null || MainSystem.Exists) {
                DestroyImmediate(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Current = this;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        private void Start() {
            if (Current != this) {
                return;
            }

            SceneManager.LoadScene(_bootSceneName);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            if (Current != this) {
                return;
            }

            Current = null;
        }
    }
}