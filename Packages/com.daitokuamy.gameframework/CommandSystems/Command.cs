using GameFramework.Core;

namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンドクラス
    /// </summary>
    public abstract class Command : ICommand {
        private CommandManager _manager;
        private CommandState _currentState = CommandState.Invalid;
        private DisposableScope _initializeScope;
        private DisposableScope _startScope;

        /// <summary>現在状態</summary>
        CommandState ICommand.CurrentState => _currentState;

        /// <summary>優先順位(0以上, 高いほうが優先度高)</summary>
        public virtual int Priority => 0;
        /// <summary>スタンバイ中の他Commandの実行をBlockするか</summary>
        public virtual bool BlockStandbyOthers => false;
        /// <summary>実行中のCommandが無くなるまでスタンバイし続けるか</summary>
        public virtual bool WaitExecutionOthers => false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        void ICommand.Initialize(CommandManager manager) {
            if (_currentState != CommandState.Invalid) {
                return;
            }

            _initializeScope = new DisposableScope();
            _manager = manager;
            InitializeInternal(_initializeScope);
            _currentState = CommandState.Standby;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <returns>trueを返すと実行開始</returns>
        bool ICommand.Start() {
            if (_currentState != CommandState.Standby) {
                return false;
            }
            
            if (!CheckStartInternal()) {
                return false;
            }
            
            _startScope = new DisposableScope();
            StartInternal(_startScope);
            _currentState = CommandState.Executing;
            return true;
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
            _manager = null;
            _currentState = CommandState.Destroyed;
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="scope">Destroyで消えるScope</param>
        protected virtual void InitializeInternal(IScope scope) {
        }

        /// <summary>
        /// 開始チェック処理
        /// </summary>
        /// <returns>trueを返すと実行開始</returns>
        protected virtual bool CheckStartInternal() {
            return true;
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <param name="scope">Finishで消えるScope</param>
        protected virtual void StartInternal(IScope scope) {
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

        /// <summary>
        /// 自分より優先度の低いCommandをキャンセルさせる
        /// </summary>
        protected void CancelLowPriorityCommands() {
            if (_manager == null) {
                return;
            }
            
            _manager.CancelCommands(Priority);
        }
    }
}