using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GameFramework.Core {
    /// <summary>
    /// 非同期処理汎用オペレーター
    /// </summary>
    /// 
    /// <example>
    /// public class OperatorClass {
    ///     public AsyncOperationHandle HogeAsync() {
    ///         var op = new AsyncOperator();
    ///         // 非同期処理
    ///         InternalAsync(() => {
    ///             // 非同期完了
    ///             op.Completed();
    ///         }
    ///         return op;
    ///     }
    /// }
    ///
    /// public class SampleClass {
    ///     public IEnumerator SetupRoutine() {
    ///         var operatorClass = new OperatorClass();
    ///         var handle = operatorClass.HogeAsync();
    ///         yield return handle;
    ///     }
    /// }
    /// </example>
    public class AsyncOperator {
        // 正常終了か
        public bool IsCompleted { get; private set; }
        // エラー内容
        public Exception Exception { get; private set; }
        // エラー終了か
        public bool IsError => Exception != null;
        // 完了しているか
        public bool IsDone => IsCompleted || IsError;

        // 完了通知イベント
        public event Action OnCompletedEvent;
        // キャンセル通知イベント
        public event Action<Exception> OnAbortedEvent;

        /// <summary>
        /// 完了済みOperatorの生成
        /// </summary>
        public static AsyncOperator CreateCompletedOperator() {
            var op = new AsyncOperator();
            op.Completed();
            return op;
        }

        /// <summary>
        /// エラー済みOperatorの生成
        /// </summary>
        public static AsyncOperator CreateAbortedOperator(Exception exception) {
            var op = new AsyncOperator();
            op.Aborted(exception);
            return op;
        }

        /// <summary>
        /// ハンドルへの暗黙型変換
        /// </summary>
        public static implicit operator AsyncOperationHandle(AsyncOperator source) {
            return source.GetHandle();
        }

        /// <summary>
        /// ハンドルの取得
        /// </summary>
        public AsyncOperationHandle GetHandle() {
            return new AsyncOperationHandle(this);
        }

        /// <summary>
        /// 完了時に呼び出す処理
        /// </summary>
        public void Completed() {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate completion action.");
            }

            IsCompleted = true;
            OnCompletedEvent?.Invoke();
            OnCompletedEvent = null;
            OnAbortedEvent = null;
        }

        /// <summary>
        /// エラー時に呼び出す処理
        /// </summary>
        /// <param name="exception">エラー原因</param>
        public void Aborted(Exception exception = null) {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate abort action.");
            }

            if (exception == null) {
                exception = new OperationCanceledException("Canceled operation");
            }

            Exception = exception;
            OnAbortedEvent?.Invoke(exception);
            OnCompletedEvent = null;
            OnAbortedEvent = null;
        }
    }

    /// <summary>
    /// 非同期処理ハンドル用オペレーター
    /// </summary>
    public class AsyncOperator<T> {
        // 結果
        public T Result { get; private set; }
        // 正常終了か
        public bool IsCompleted { get; private set; }
        // エラー内容
        public Exception Exception { get; private set; }
        // エラー終了か
        public bool IsError => Exception != null;
        // 完了しているか
        public bool IsDone => IsCompleted || IsError;

        // 完了通知イベント
        public event Action<T> OnCompletedEvent;
        // エラー通知イベント
        public event Action<Exception> OnAbortedEvent;

        /// <summary>
        /// ハンドルへの暗黙型変換
        /// </summary>
        public static implicit operator AsyncOperationHandle<T>(AsyncOperator<T> source) {
            return source.GetHandle();
        }

        /// <summary>
        /// 完了済みOperatorの生成
        /// </summary>
        public static AsyncOperator<T> CreateCompletedOperator(T result) {
            var op = new AsyncOperator<T>();
            op.Completed(result);
            return op;
        }

        /// <summary>
        /// エラー済みOperatorの生成
        /// </summary>
        public static AsyncOperator<T> CreateAbortedOperator(Exception exception) {
            var op = new AsyncOperator<T>();
            op.Aborted(exception);
            return op;
        }

        /// <summary>
        /// ハンドルの取得
        /// </summary>
        public AsyncOperationHandle<T> GetHandle() {
            return new AsyncOperationHandle<T>(this);
        }

        /// <summary>
        /// 完了時に呼び出す処理
        /// </summary>
        public void Completed(T result) {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate completion action.");
            }

            Result = result;
            IsCompleted = true;
            OnCompletedEvent?.Invoke(result);
            OnCompletedEvent = null;
            OnAbortedEvent = null;
        }

        /// <summary>
        /// エラー時に呼び出す処理
        /// </summary>
        /// <param name="exception">エラー原因</param>
        public void Aborted(Exception exception = null) {
            if (IsDone) {
                throw new InvalidOperationException("Duplicate abort action.");
            }

            if (exception == null) {
                exception = new OperationCanceledException("Canceled operation");
            }

            Exception = exception;
            OnAbortedEvent?.Invoke(exception);
            OnCompletedEvent = null;
            OnAbortedEvent = null;
        }
    }

    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public readonly struct AsyncOperationHandle : IProcess {
        /// <summary>キャンセル済みHandle</summary>
        public static readonly AsyncOperationHandle CanceledHandle = new(new OperationCanceledException());
        /// <summary>完了済みHandle</summary>
        public static readonly AsyncOperationHandle CompletedHandle = new();

        private readonly AsyncOperator _asyncOperator;
        private readonly Exception _exception;

        // 完了しているか
        public bool IsDone => _asyncOperator == null || _asyncOperator.IsDone || IsError;
        // エラー終了か
        public bool IsError => Exception != null;
        // キャンセル時のエラー
        public Exception Exception => _asyncOperator?.Exception ?? _exception;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asyncOperator">非同期通知用インスタンス</param>
        internal AsyncOperationHandle(AsyncOperator asyncOperator) {
            _asyncOperator = asyncOperator;
            _exception = null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exception">例外</param>
        internal AsyncOperationHandle(Exception exception) {
            _asyncOperator = null;
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

        /// <summary>
        /// 通知の購読
        /// </summary>
        /// <param name="onCompleted">完了時通知</param>
        /// <param name="onError">エラー時通知</param>
        public void ListenTo(Action onCompleted, Action<Exception> onError = null) {
            if (_asyncOperator == null || _asyncOperator.IsDone) {
                var exception = Exception;
                if (exception != null) {
                    onError?.Invoke(exception);
                }
                else {
                    onCompleted?.Invoke();
                }
                
                return;
            }

            if (onCompleted != null) {
                _asyncOperator.OnCompletedEvent += onCompleted;
            }

            if (onError != null) {
                _asyncOperator.OnAbortedEvent += onError;
            }
        }
    }

    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public readonly struct AsyncOperationHandle<T> : IProcess<T> {
        /// <summary>キャンセル済みHandle</summary>
        public static readonly AsyncOperationHandle<T> CanceledHandle = new();
        /// <summary>完了済みHandle</summary>
        public static readonly AsyncOperationHandle<T> CompletedHandle = new();

        private readonly AsyncOperator<T> _asyncOperator;
        private readonly Exception _exception;

        // 結果
        public T Result => _asyncOperator != null ? _asyncOperator.Result : default;
        // 完了しているか
        public bool IsDone => _asyncOperator == null || _asyncOperator.IsDone || IsError;
        // エラー終了か
        public bool IsError => Exception != null;
        // キャンセル時のエラー
        public Exception Exception => _asyncOperator?.Exception ?? _exception;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asyncOperator">非同期通知用インスタンス</param>
        internal AsyncOperationHandle(AsyncOperator<T> asyncOperator) {
            _asyncOperator = asyncOperator;
            _exception = null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exception">例外</param>
        internal AsyncOperationHandle(Exception exception) {
            _asyncOperator = null;
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

        /// <summary>
        /// 通知の購読
        /// </summary>
        /// <param name="onCompleted">完了時通知</param>
        /// <param name="onError">エラー時通知</param>
        public void ListenTo(Action<T> onCompleted, Action<Exception> onError = null) {
            if (_asyncOperator == null || _asyncOperator.IsDone) {
                var exception = Exception;
                if (exception != null) {
                    onError?.Invoke(exception);
                }
                else {
                    onCompleted?.Invoke(Result);
                }

                return;
            }

            if (onCompleted != null) {
                _asyncOperator.OnCompletedEvent += onCompleted;
            }

            if (onError != null) {
                _asyncOperator.OnAbortedEvent += onError;
            }
        }
    }

    /// <summary>
    /// AsyncOperationHandle用のAwaiter
    /// </summary>
    public readonly struct AsyncOperationHandleAwaiter : INotifyCompletion {
        private readonly AsyncOperationHandle _handle;

        /// <summary>完了しているか</summary>
        public bool IsCompleted {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle.IsDone;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncOperationHandleAwaiter(AsyncOperationHandle handle) {
            _handle = handle;
        }
        
        /// <summary>
        /// Await開始時の処理
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation) {
            _handle.ListenTo(continuation);
        }

        /// <summary>
        /// 結果の取得
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult() {
        }
    }

    /// <summary>
    /// AsyncOperationHandle用のAwaiter
    /// </summary>
    public readonly struct AsyncOperationHandleAwaiter<T> : INotifyCompletion {
        private readonly AsyncOperationHandle<T> _handle;

        /// <summary>完了しているか</summary>
        public bool IsCompleted {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle.IsDone;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncOperationHandleAwaiter(AsyncOperationHandle<T> handle) {
            _handle = handle;
        }
        
        /// <summary>
        /// Await開始時の処理
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation) {
            _handle.ListenTo(_ => continuation());
        }

        /// <summary>
        /// 結果の取得
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetResult() {
            return _handle.Result;
        }
    }
}