using GameFramework.Core;

namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンドクラス
    /// </summary>
    public abstract class Command : ICommand {
        private CommandState _currentState = CommandState.Invalid;
        private DisposableScope _initializeScope;
        private DisposableScope _startScope;

        /// <summary>現在状態</summary>
        CommandState ICommand.CurrentState => _currentState;

        /// <summary>優先順位(0以上, 高いほうが優先度高)</summary>
        public virtual int Priority => 0;
        /// <summary>割り込みするか(自分より優先度の低い物が実行中だった場合、強制的に停止して実行する)</summary>
        public virtual bool Interrupt => false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICommand.Initialize() {
            if (_currentState != CommandState.Invalid) {
                return;
            }

            _initializeScope = new DisposableScope();
            InitializeInternal(_initializeScope);
            _currentState = CommandState.Standby;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <returns>trueを返すと実行継続</returns>
        bool ICommand.Start() {
            if (_currentState != CommandState.Standby) {
                return true;
            }
            
            _startScope = new DisposableScope();
            var result = StartInternal(_startScope);
            _currentState = CommandState.Executing;
            return result;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <returns>trueを返すと継続</returns>
        bool ICommand.Update() {
            if (_currentState != CommandState.Executing) {
                return false;
            }

            return UpdateInternal();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        void ICommand.Finish() {
            if (_currentState >= CommandState.Finished) {
                return;
            }

            if (_startScope != null) {
                _startScope.Dispose();
                _startScope = null;
            }

            FinishInternal();
            _currentState = CommandState.Finished;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        void ICommand.Destroy() {
            if (_currentState >= CommandState.Destroyed) {
                return;
            }

            if (_currentState < CommandState.Finished) {
                ((ICommand)this).Finish();
            }

            if (_initializeScope != null) {
                _initializeScope.Dispose();
                _initializeScope = null;
            }

            DestroyInternal();
            _currentState = CommandState.Destroyed;
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="scope">Destroyで消えるScope</param>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <param name="scope">Finishで消えるScope</param>
        /// <returns>trueを返すと実行継続</returns>
        protected virtual bool StartInternal(IScope scope) {
            return true;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <returns>trueを返すと実行継続</returns>
        protected virtual bool UpdateInternal() {
            return false;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        protected virtual void FinishInternal() {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DestroyInternal() {
        }
    }
}