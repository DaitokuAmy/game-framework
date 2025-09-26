using System;

namespace GameFramework.Core {
    /// <summary>
    /// インスタンス提供用のロケーター
    /// </summary>
    public interface IServiceContainer : IServiceResolver, IDisposable {
        /// <summary>親のDisposeに合わせて消えるか</summary>
        bool WithParentDispose { get; }
        
        /// <summary>
        /// コンテナのクリア
        /// </summary>
        void Clear();

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        IDisposable Register<TInterface1, TInterface2, TInterface3, TInterface4, T>(Func<T> createFunc = null);

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        IDisposable Register<TInterface1, TInterface2, TInterface3, T>(Func<T> createFunc = null);

        /// <summary>
        /// サービスの登録
        /// </summary>
        /// <param name="createFunc">生成処理</param>
        /// <returns>登録解除用Disposable</returns>
        IDisposable Register<TInterface1, TInterface2, T>(Func<T> createFunc = null);
        
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