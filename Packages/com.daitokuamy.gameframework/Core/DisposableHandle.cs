using System;

namespace GameFramework.Core {
    /// <summary>
    /// DisposableHandleで管理するインスタンスのインターフェース
    /// </summary>
    public interface IDisposableHandleContent : IDisposable {
        /// <summary>有効か</summary>
        bool IsValid { get; }
        /// <summary>完了しているか</summary>
        bool IsDisposed { get; }
    }
    
    /// <summary>
    /// Disposableを管理するHandle
    /// </summary>
    public struct DisposableHandle : IDisposable {
        /// <summary>無効なハンドル</summary>
        public static readonly DisposableHandle Empty = new();
        
        /// <summary>有効なハンドルか</summary>
        public bool IsValid => _content?.IsValid ?? false;
        /// <summary>廃棄済みか</summary>
        public bool IsDisposed => _content?.IsDisposed ?? true;

        private IDisposableHandleContent _content;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="content">ハンドル管理するインスタンスのインターフェース</param>
        public DisposableHandle(IDisposableHandleContent content) {
            _content = content;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (!IsValid || IsDisposed) {
                return;
            }
            
            _content.Dispose();
        }
    }
}