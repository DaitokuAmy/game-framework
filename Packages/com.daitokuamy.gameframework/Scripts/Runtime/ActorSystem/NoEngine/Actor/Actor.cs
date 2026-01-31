using System;
using System.Collections.Generic;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクター本体
    /// </summary>
    public sealed class Actor<TKey> : IActorRuntime<TKey>, IActorCommandInputPort, IActorSignalInputPort {
        private readonly ActorBuffer<ActorCommand> _commandBuffer = new();
        private readonly ActorBuffer<ActorSignal> _signalBuffer = new();
        private readonly List<ActorCommand> _workCommands = new();
        private readonly List<ActorSignal> _workSignals = new();

        private IActorStateMachine _stateMachine;
        private IActorController _controller;
        private IActorReceiver _receiver;
        private IActorModel _model;
        private IActorPresenter _presenter;
        private IActorView _view;
        private bool _initialized;
        private bool _disposed;
        private bool _active;

        /// <summary>識別ID</summary>
        public TKey Id { get; private set; }
        /// <summary>アクティブ状態</summary>
        public bool IsActive => _initialized && !_disposed && _active;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Actor() {
        }

        /// <inheritdoc/>
        void IDisposable.Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            ResetActorInterface(ref _receiver, DetachedAction);
            ResetActorInterface(ref _view, null);
            ResetActorInterface(ref _presenter, null);
            ResetActorInterface(ref _model, null);
            ResetActorInterface(ref _controller, DetachedAction);

            _active = false;
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.Initialize(TKey id) {
            if (_initialized) {
                throw new InvalidOperationException("Already initialized.");
            }

            _initialized = true;

            Id = id;
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.Terminate() {
            if (!_initialized) {
                throw new InvalidOperationException("Not initialized.");
            }

            _initialized = false;

            Id = default;

            ResetActorInterface(ref _receiver, DetachedAction);
            ResetActorInterface(ref _view, null);
            ResetActorInterface(ref _presenter, null);
            ResetActorInterface(ref _model, null);
            ResetActorInterface(ref _controller, DetachedAction);

            _active = false;
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.UpdateController(float deltaTime) {
            if (!IsActive) {
                return;
            }

            _controller?.Update(deltaTime);
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.UpdateStateMachine(float deltaTime) {
            if (!IsActive) {
                return;
            }

            _commandBuffer.GetBufferedContents(_workCommands);
            _signalBuffer.GetBufferedContents(_workSignals);
            _stateMachine.Update(_workCommands, _workSignals, deltaTime);
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.UpdateModel(float deltaTime) {
            if (!IsActive) {
                return;
            }

            _model?.Update(deltaTime);
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.UpdatePresenter(float deltaTime) {
            if (!IsActive) {
                return;
            }

            _presenter?.Update(deltaTime);
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.UpdateView(float deltaTime) {
            if (!IsActive) {
                return;
            }

            _view?.Update(deltaTime);
        }

        /// <inheritdoc/>
        void IActorRuntime<TKey>.UpdateReceiver(float deltaTime) {
            if (!IsActive) {
                return;
            }

            _receiver?.Update(deltaTime);
        }

        /// <summary>
        /// Controllerの取得
        /// </summary>
        public T GetController<T>()
            where T : class {
            return _controller as T;
        }

        /// <summary>
        /// Receiverの取得
        /// </summary>
        public T GetReceiver<T>()
            where T : class {
            return _receiver as T;
        }

        /// <summary>
        /// Presenterの取得
        /// </summary>
        public T GetPresenter<T>()
            where T : class {
            return _presenter as T;
        }

        /// <summary>
        /// Modelの取得
        /// </summary>
        public T GetModel<T>()
            where T : class {
            return _model as T;
        }

        /// <summary>
        /// Viewの取得
        /// </summary>
        public T GetView<T>()
            where T : class {
            return _view as T;
        }

        /// <summary>
        /// アクティブ状態の設定
        /// </summary>
        public void SetActive(bool active) {
            if (_active == active || !_initialized || _disposed) {
                return;
            }

            _active = active;
            if (_active) {
                _model?.Activate();
                _view?.Activate();
                _controller?.Activate();
                _receiver?.Activate();
                _presenter?.Activate();
            }
            else {
                _presenter?.Deactivate();
                _receiver?.Deactivate();
                _controller?.Deactivate();
                _view?.Deactivate();
                _model?.Deactivate();
            }
        }

        /// <summary>
        /// StateMachineの初期化
        /// </summary>
        /// <param name="startStateType">開始ステートのタイプ</param>
        /// <param name="states">登録するステート一覧</param>
        public void SetupStateMachine<TBlackboard>(Type startStateType, params ActorState<TKey, TBlackboard>[] states)
            where TBlackboard : class, IActorStateBlackboard, new() {
            var stateMachine = new ActorStateMachine<TKey, TBlackboard>(this);
            stateMachine.SetStates(startStateType, states);
            _stateMachine = stateMachine;
        }

        /// <summary>
        /// Controllerの設定
        /// </summary>
        public void SetController(IActorController controller, bool prevDispose = true) {
            ResetActorInterface(ref _controller, DetachedAction, prevDispose);
            SetActorInterface(controller, ref _controller, AttachedAction);
        }

        /// <summary>
        /// Receiverの設定
        /// </summary>
        public void SetReceiver(IActorReceiver receiver, bool prevDispose = true) {
            ResetActorInterface(ref _receiver, DetachedAction, prevDispose);
            SetActorInterface(receiver, ref _receiver, AttachedAction);
        }

        /// <summary>
        /// Modelの設定
        /// </summary>
        public void SetModel(IActorModel model, bool prevDispose = true) {
            ResetActorInterface(ref _model, null, prevDispose);
            SetActorInterface(model, ref _model, null);
        }

        /// <summary>
        /// Presenterの設定
        /// </summary>
        public void SetPresenter(IActorPresenter presenter, bool prevDispose = true) {
            ResetActorInterface(ref _presenter, null, prevDispose);
            SetActorInterface(presenter, ref _presenter, null);
        }

        /// <summary>
        /// Viewの設定
        /// </summary>
        public void SetView(IActorView view, bool prevDispose = true) {
            ResetActorInterface(ref _view, null, prevDispose);
            SetActorInterface(view, ref _view, null);
        }

        /// <inheritdoc/>
        public T CreateCommand<T>()
            where T : ActorCommand, new() {
            return _commandBuffer.CreateContent<T>();
        }

        /// <inheritdoc/>
        public void AddCommand(ActorCommand command) {
            _commandBuffer.AddContent(command);
        }

        /// <inheritdoc/>
        public T CreateSignal<T>()
            where T : ActorSignal, new() {
            return _signalBuffer.CreateContent<T>();
        }

        /// <inheritdoc/>
        public void AddSignal(ActorSignal signal) {
            _signalBuffer.AddContent(signal);
        }

        /// <summary>
        /// IActorInterfaceの設定
        /// </summary>
        /// <param name="target">設定対象</param>
        /// <param name="field">設定先のフィールド</param>
        /// <param name="attachedAction">アタッチされた際の処理</param>
        private void SetActorInterface<T>(T target, ref T field, Action<T> attachedAction)
            where T : class, IActorInterface {
            if (!_initialized || _disposed) {
                return;
            }

            field = target;
            if (field != null) {
                attachedAction?.Invoke(field);

                if (_active) {
                    field.Activate();
                }
            }
        }

        /// <summary>
        /// IActorInterfaceの除外処理
        /// </summary>
        private void ResetActorInterface<T>(ref T field, Action<T> detachedAction, bool dispose = true)
            where T : class, IActorInterface {
            if (field == null) {
                return;
            }

            if (_active) {
                field.Deactivate();
            }

            detachedAction?.Invoke(field);

            if (dispose) {
                field.Dispose();
            }

            field = null;
        }

        /// <summary>
        /// ActorControllerをDetachする際の処理
        /// </summary>
        private void DetachedAction(IActorController x) {
            x.Detached();
        }

        /// <summary>
        /// ActorControllerをAttachする際の処理
        /// </summary>
        private void AttachedAction(IActorController x) {
            x.Attached(this);
        }

        /// <summary>
        /// ActorReceiverをDetachする際の処理
        /// </summary>
        private void DetachedAction(IActorReceiver x) {
            x.Detached();
        }

        /// <summary>
        /// ActorReceiverをAttachする際の処理
        /// </summary>
        private void AttachedAction(IActorReceiver x) {
            x.Attached(this);
        }
    }
}