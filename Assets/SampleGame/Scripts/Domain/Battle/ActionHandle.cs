using System;
using System.Collections;
using GameFramework.Core;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// アクション情報
    /// </summary>
    internal class ActionInfo : IProcess {
        private bool _isDone;
        private Exception _exception;
        
        object IEnumerator.Current => null;
        bool IProcess.IsDone => _isDone;
        Exception IProcess.Exception => _exception;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActionInfo() {
        }

        /// <summary>
        /// 完了チェック
        /// </summary>
        bool IEnumerator.MoveNext() {
            return !_isDone;
        }

        /// <summary>
        /// リセット
        /// </summary>
        void IEnumerator.Reset() {
        }
        
        /// <summary>
        /// アクションの完了
        /// </summary>
        public void Complete() {
            if (_isDone) {
                return;
            }

            _isDone = true;
        }

        /// <summary>
        /// アクションのキャンセル
        /// </summary>
        public void Cancel() {
            if (_isDone) {
                return;
            }

            _exception = new OperationCanceledException();
            _isDone = true;
        }
    }
    
    /// <summary>
    /// アクション管理用のハンドル
    /// </summary>
    public struct ActionHandle {
        private ActionInfo _actionInfo;

        /// <summary>有効なハンドルか</summary>
        public bool IsValid => _actionInfo != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal ActionHandle(ActionInfo actionInfo) {
            _actionInfo = actionInfo;
        }
        
        /// <summary>
        /// アクションの完了
        /// </summary>
        public void Complete() {
            _actionInfo?.Complete();
        }

        /// <summary>
        /// アクションのキャンセル
        /// </summary>
        public void Cancel() {
            _actionInfo.Cancel();
        }
    }
}