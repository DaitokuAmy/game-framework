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
        /// サービスの設定
        /// </summary>
        /// <param name="type">紐づけ用の型</param>
        /// <param name="service">登録するインスタンス</param>
        void Set<TClass>(Type type, TClass service)
            where TClass : class;

        /// <summary>
        /// サービスの設定
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        void Set<T, TClass>(TClass service)
            where TClass : class;

        /// <summary>
        /// サービスの設定
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        void Set<TClass>(TClass service)
            where TClass : class;

        /// <summary>
        /// サービスの設定(複数登録するバージョン）
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        /// <param name="index">インデックス</param>
        void Set<T, TClass>(TClass service, int index)
            where TClass : class;

        /// <summary>
        /// サービスの設定(複数登録するバージョン）
        /// </summary>
        /// <param name="type">紐づけ用の型</param>
        /// <param name="service">登録するインスタンス</param>
        /// <param name="index">インデックス</param>
        void Set<TClass>(Type type, TClass service, int index)
            where TClass : class;

        /// <summary>
        /// サービスの設定(複数登録するバージョン）
        /// </summary>
        /// <param name="service">登録するインスタンス</param>
        /// <param name="index">インデックス</param>
        void Set<TClass>(TClass service, int index)
            where TClass : class;

        /// <summary>
        /// サービスの取得
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// </summary>
        object Get(Type type);

        /// <summary>
        /// サービスの取得
        /// </summary>
        T Get<T>();

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <param name="index">インデックス</param>
        object Get(Type type, int index);

        /// <summary>
        /// サービスの取得(複数登録するバージョン）
        /// </summary>
        /// <param name="index">インデックス</param>
        T Get<T>(int index);

        /// <summary>
        /// サービスの削除
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// </summary>
        void Remove(Type type);

        /// <summary>
        /// サービスの削除
        /// </summary>
        void Remove<T>();

        /// <summary>
        /// サービスの削除
        /// <param name="type">登録したインスタンスのタイプ</param>
        /// <param name="index">インデックス</param>
        /// </summary>
        void Remove(Type type, int index);

        /// <summary>
        /// サービスの削除
        /// <param name="index">インデックス</param>
        /// </summary>
        void Remove<T>(int index);
    }
}