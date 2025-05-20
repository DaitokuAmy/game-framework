using System;

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のロケーター
    /// </summary>
    public interface IServiceContainer : IDisposable {
        /// <summary>
        /// コンテナのクリア
        /// </summary>
        void Clear();
        
        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        IDisposable Register<TInterface, T>(Func<T> createFunc = null);
        
        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        IDisposable Register<T>(Func<T> createFunc = null);
        
        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="service">インスタンス</param>
        /// <returns>登録解除用Disposable</returns>
        IDisposable RegisterInstance<T>(object service);
        
        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="service">インスタンス</param>
        /// <returns>登録解除用Disposable</returns>
        IDisposable RegisterInstance(object service);

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
        /// サービスの削除
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// </summary>
        void Remove(Type type);

        /// <summary>
        /// サービスの削除
        /// </summary>
        void Remove<T>();
    }
}