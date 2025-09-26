using System;

namespace GameFramework.Core {
    /// <summary>
    /// Service提供用のインターフェース
    /// </summary>
    public interface IServiceResolver {
        /// <summary>
        /// サービスの取得
        /// </summary>
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <returns>サービスインスタンス</returns>
        object Resolve(Type type);

        /// <summary>
        /// サービスの取得
        /// </summary>
        /// <returns>サービスインスタンス</returns>
        T Resolve<T>();

        /// <summary>
        /// インスタンスにServiceを注入
        /// </summary>
        void Inject(object instance);
    }
}