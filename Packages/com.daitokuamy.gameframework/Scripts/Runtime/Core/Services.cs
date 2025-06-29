using System;

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

        /// <summary>
        /// シングルトンインスタンスごと解放
        /// </summary>
        public static void Cleanup() {
            s_instance?.Dispose();
            s_instance = null;
        }
    }
}