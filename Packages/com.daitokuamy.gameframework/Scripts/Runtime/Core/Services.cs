using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のルートコンテナ
    /// </summary>
    public sealed class Services : ServiceContainer {
        private static IServiceContainer s_instance;

        /// <summary>シングルトン用インスタンス取得</summary>
        public static IServiceContainer Instance {
            get {
                if (s_instance == null) {
                    s_instance = new Services();
                }

                return s_instance;
            }
        }

        /// <summary>
        /// サービスの取得
        /// </summary>
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <returns>サービスインスタンス</returns>
        public static object Resolve(Type type) {
            return Instance.Resolve(type);
        }

        /// <summary>
        /// サービスの取得
        /// </summary>
        /// <returns>サービスインスタンス</returns>
        public static T Resolve<T>() {
            return Instance.Resolve<T>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ起動時の処理
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnInitializeOnLoad() {
            // Play/Edit切り替わり時にインスタンスを解放
            EditorApplication.playModeStateChanged += change => {
                switch (change) {
                    case PlayModeStateChange.EnteredEditMode:
                    case PlayModeStateChange.ExitingEditMode:
                        s_instance?.Dispose();
                        s_instance = null;
                        break;
                }
            };
        }
#endif
    }
}