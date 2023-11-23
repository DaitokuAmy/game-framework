using System;
using System.Collections;

namespace GameFramework.Core {
    /// <summary>
    /// 非同期的な状態を返すためのインターフェース
    /// </summary>
    public interface IAsyncStatusProvider {
        /// <summary>完了しているか</summary>
        public bool IsDone { get; }
        /// <summary>エラー状態</summary>
        public Exception Exception { get; }
    }
    
    /// <summary>
    /// 非同期的な状態を返すためのインターフェース
    /// </summary>
    public interface IAsyncStatusProvider<T> : IAsyncStatusProvider {
        /// <summary>結果</summary>
        public T Result { get; }
    }

    /// <summary>
    /// 非同期的な状態を返すための汎用クラス
    /// </summary>
    public class AsyncStatusProvider : IAsyncStatusProvider {
        private readonly Func<bool> _onCheckIsDone;
        private readonly Func<Exception> _onCheckException;
        
        /// <summary>完了しているか</summary>
        bool IAsyncStatusProvider.IsDone => _onCheckIsDone?.Invoke() ?? true;
        /// <summary>エラー状態</summary>
        Exception IAsyncStatusProvider.Exception => _onCheckException?.Invoke();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onCheckIsDoneFunc">完了判定用の関数</param>
        /// <param name="onCheckException">エラー判定用の関数</param>
        public AsyncStatusProvider(Func<bool> onCheckIsDoneFunc, Func<Exception> onCheckException) {
            _onCheckIsDone = onCheckIsDoneFunc;
            _onCheckException = onCheckException;
        }

        /// <summary>
        /// ハンドルへの暗黙型変換
        /// </summary>
        public static implicit operator AsyncStatusHandle(AsyncStatusProvider source) {
            return new AsyncStatusHandle(source);
        }
    }

    /// <summary>
    /// 非同期的な状態を返すための汎用クラス
    /// </summary>
    public class AsyncStatusProvider<T> : IAsyncStatusProvider<T> {
        private readonly Func<bool> _onCheckIsDone;
        private readonly Func<Exception> _onCheckException;
        private readonly Func<T> _onGetResult;
        
        /// <summary>完了しているか</summary>
        bool IAsyncStatusProvider.IsDone => _onCheckIsDone?.Invoke() ?? true;
        /// <summary>エラー状態</summary>
        Exception IAsyncStatusProvider.Exception => _onCheckException?.Invoke();
        T IAsyncStatusProvider<T>.Result => _onGetResult != null ? _onGetResult.Invoke() : default;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onCheckIsDoneFunc">完了判定用の関数</param>
        /// <param name="onCheckException">エラー判定用の関数</param>
        /// <param name="onGetResult">結果取得用</param>
        public AsyncStatusProvider(Func<bool> onCheckIsDoneFunc, Func<Exception> onCheckException, Func<T> onGetResult) {
            _onCheckIsDone = onCheckIsDoneFunc;
            _onCheckException = onCheckException;
            _onGetResult = onGetResult;
        }

        /// <summary>
        /// ハンドルへの暗黙型変換
        /// </summary>
        public static implicit operator AsyncStatusHandle<T>(AsyncStatusProvider<T> source) {
            return new AsyncStatusHandle<T>(source);
        }
    }

    /// <summary>
    /// 非同期的な状態をハンドリングするための構造体
    /// </summary>
    public readonly struct AsyncStatusHandle : IProcess {
        /// <summary>キャンセル済みHandle</summary>
        public static readonly AsyncStatusHandle CanceledHandle = new(new OperationCanceledException());
        /// <summary>完了済みHandle</summary>
        public static readonly AsyncStatusHandle CompletedHandle = new();

        private readonly IAsyncStatusProvider _asyncStatusProvider;
        private readonly Exception _exception;

        /// <summary>完了しているか</summary>
        public bool IsDone => _asyncStatusProvider == null || _asyncStatusProvider.IsDone || IsError;
        /// <summary>エラー終了か</summary>
        public bool IsError => Exception != null;
        /// <summary>キャンセル時のエラー</summary>
        public Exception Exception => _asyncStatusProvider?.Exception ?? _exception;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asyncStatusProvider">非同期通知用インスタンス</param>
        internal AsyncStatusHandle(IAsyncStatusProvider asyncStatusProvider) {
            _asyncStatusProvider = asyncStatusProvider;
            _exception = null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exception">例外</param>
        internal AsyncStatusHandle(Exception exception) {
            _asyncStatusProvider = null;
            _exception = exception;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 非同期的な状態をハンドリングするための構造体
    /// </summary>
    public readonly struct AsyncStatusHandle<T> : IProcess<T> {
        /// <summary>キャンセル済みHandle</summary>
        public static readonly AsyncStatusHandle<T> CanceledHandle = new(new OperationCanceledException());
        /// <summary>完了済みHandle</summary>
        public static readonly AsyncStatusHandle<T> CompletedHandle = new();

        private readonly IAsyncStatusProvider<T> _asyncStatusProvider;
        private readonly Exception _exception;

        /// <summary>結果</summary>
        public T Result => _asyncStatusProvider == null ? default : _asyncStatusProvider.Result;
        /// <summary>完了しているか</summary>
        public bool IsDone => _asyncStatusProvider == null || _asyncStatusProvider.IsDone || IsError;
        /// <summary>エラー終了か</summary>
        public bool IsError => Exception != null;
        /// <summary>キャンセル時のエラー</summary>
        public Exception Exception => _asyncStatusProvider?.Exception ?? _exception;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asyncStatusProvider">非同期通知用インスタンス</param>
        internal AsyncStatusHandle(IAsyncStatusProvider<T> asyncStatusProvider) {
            _asyncStatusProvider = asyncStatusProvider;
            _exception = null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exception">例外</param>
        internal AsyncStatusHandle(Exception exception) {
            _asyncStatusProvider = null;
            _exception = exception;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }
    }
}