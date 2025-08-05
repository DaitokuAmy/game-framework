using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// ダイアログ状態管理用ハンドル
    /// </summary>
    public readonly struct DialogHandle : IEventProcess<int> {
        public static readonly DialogHandle Empty = new();

        private readonly UIDialogContainer.DialogInfo _info;

        /// <inheritdoc/>
        object IEnumerator.Current => null;
        /// <inheritdoc/>
        Exception IProcess.Exception => null;
        /// <inheritdoc/>
        int IProcess<int>.Result => _info?.Result ?? -1;

        /// <inheritdoc/>
        public bool IsDone => _info == null || _info.IsDone;

        /// <inheritdoc/>
        event Action<int> IEventProcess<int>.ExitEvent {
            add {
                if (_info != null) {
                    _info.SelectedIndexEvent += value;
                }
            }
            remove {
                if (_info != null) {
                    _info.SelectedIndexEvent -= value;
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal DialogHandle(UIDialogContainer.DialogInfo info) {
            _info = info;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public EventProcessAwaiter<int> GetAwaiter() {
            return new EventProcessAwaiter<int>(this);
        }
    }
    
    /// <summary>
    /// ダイアログ状態管理用ハンドル
    /// </summary>
    public struct DialogHandle<TResult> : IEventProcess<TResult> {
        public static readonly DialogHandle<TResult> Empty = new();

        private readonly UIDialogContainer.DialogInfo<TResult> _info;

        /// <inheritdoc/>
        object IEnumerator.Current => null;
        /// <inheritdoc/>
        Exception IProcess.Exception => null;
        /// <inheritdoc/>
        TResult IProcess<TResult>.Result => _info != null ? _info.GetResult() : default;

        /// <inheritdoc/>
        public bool IsDone => _info == null || _info.IsDone;

        /// <inheritdoc/>
        event Action<TResult> IEventProcess<TResult>.ExitEvent {
            add {
                if (_info != null) {
                    _info.SelectedEvent += value;
                }
            }
            remove {
                if (_info != null) {
                    _info.SelectedEvent -= value;
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal DialogHandle(UIDialogContainer.DialogInfo<TResult> info) {
            _info = info;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset() {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public EventProcessAwaiter<TResult> GetAwaiter() {
            return new EventProcessAwaiter<TResult>(this);
        }
    }
}