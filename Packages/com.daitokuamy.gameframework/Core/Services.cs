using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のルートコンテナ
    /// </summary>
    public sealed class Services : ServiceContainer {
        // シングルトン用インスタンス
        private static IServiceContainer s_instance;

        // シングルトン用インスタンス取得
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
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// </summary>
        public static object Get(Type type) {
            return Instance.Get(type);
        }

        /// <summary>
        /// サービスの取得
        /// </summary>
        public static T Get<T>() {
            return Instance.Get<T>();
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <param name="index">インデックス</param>
        public static object Get(Type type, int index) {
            return Instance.Get(type, index);
        }

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="index">インデックス</param>
        public static T Get<T>(int index) {
            return Instance.Get<T>(index);
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