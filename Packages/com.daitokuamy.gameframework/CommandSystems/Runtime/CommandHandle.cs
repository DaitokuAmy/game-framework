using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンドのハンドル
    /// </summary>
    public struct CommandHandle : IProcess, IDisposable {
        /// <summary>空のコマンド</summary>
        public static readonly CommandHandle Empty = new();

        private readonly ICommand _command;
        
        /// <summary>現在の値(未使用)</summary>
        object IEnumerator.Current => null;
        
        /// <summary>例外</summary>
        public Exception Exception => _command?.Exception;
        /// <summary>有効なハンドルか</summary>
        public bool IsValid => _command != null;
        /// <summary>現在のコマンド状態</summary>
        public CommandState CurrentState => _command?.CurrentState ?? CommandState.Invalid;
        /// <summary>完了しているか</summary>
        public bool IsDone => !IsValid || (CurrentState <= CommandState.Invalid || CurrentState >= CommandState.Destroyed);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommandHandle(ICommand command) {
            _command = command;
        }

        /// <summary>
        /// 継続中か
        /// </summary>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <summary>
        /// 未対応
        /// </summary>
        void IEnumerator.Reset() {}
        
        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (!IsValid) {
                return;
            }
            
            _command.Destroy();
        }
    }
}