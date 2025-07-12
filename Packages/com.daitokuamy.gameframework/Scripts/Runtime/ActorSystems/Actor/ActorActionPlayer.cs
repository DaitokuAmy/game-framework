using System;
using System.Collections.Generic;
using GameFramework.CoroutineSystems;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// ActorActionを再生するためのクラス
    /// </summary>
    public class ActorActionPlayer : IDisposable {
        /// <summary>
        /// アクターアクション再生中情報
        /// </summary>
        internal class PlayingInfo {
            private readonly CoroutineRunner _runner;
            private Coroutine _coroutine;
            private IActorAction _action;
            private IActorActionHandler _handler;

            /// <summary>完了しているか</summary>
            public bool IsDone => _coroutine == null || _coroutine.IsDone;

            /// <summary>終了通知</summary>
            public event Action FinishedEvent;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public PlayingInfo(CoroutineRunner runner, Coroutine coroutine, IActorAction action,
                IActorActionHandler handler) {
                _runner = runner;
                _coroutine = coroutine;
                _action = action;
                _handler = handler;
            }

            /// <summary>
            /// 更新処理
            /// </summary>
            public void Update(float deltaTime) {
                if (IsDone) {
                    return;
                }

                _handler.SetDeltaTime(deltaTime);
            }

            /// <summary>
            /// アクションの遷移
            /// </summary>
            public bool Next(object[] args) {
                if (IsDone) {
                    return false;
                }
                
                return _handler.Next(args);
            }

            /// <summary>
            /// キャンセル処理
            /// </summary>
            public void Cancel() {
                if (IsDone) {
                    return;
                }

                _runner.StopCoroutine(_coroutine);
                _coroutine = null;
                _handler.Cancel(_action);
                _handler.Exit(_action);
                _handler = null;
                _action = null;

                FinishedEvent?.Invoke();
                FinishedEvent = null;
            }

            /// <summary>
            /// 終了処理
            /// </summary>
            public void Exit() {
                if (_coroutine == null) {
                    return;
                }

                _runner.StopCoroutine(_coroutine);
                _coroutine = null;
                _handler.Exit(_action);
                _handler = null;
                _action = null;

                FinishedEvent?.Invoke();
                FinishedEvent = null;
            }
        }

        private readonly CoroutineRunner _coroutineRunner = new();
        private readonly Dictionary<Type, IActorActionHandler> _handlers = new();

        private PlayingInfo _playingInfo;
        
        /// <summary>アクション終了時通知</summary>
        public event Action<IActorAction, float> FinishedEvent;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            StopCurrentAction();
            _handlers.Clear();
            _coroutineRunner.Dispose();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            if (_playingInfo != null) {
                _playingInfo.Update(deltaTime);
            }

            _coroutineRunner.Update();

            if (_playingInfo != null) {
                if (_playingInfo.IsDone) {
                    _playingInfo.Exit();
                }
            }
        }

        /// <summary>
        /// ハンドラーの設定
        /// </summary>
        public void SetHandler<TAction, TActionHandler>(TActionHandler handler)
            where TAction : IActorAction
            where TActionHandler : ActorActionHandler<TAction> {
            _handlers[typeof(TAction)] = handler;
        }

        /// <summary>
        /// ハンドラーのクリア
        /// </summary>
        public void ClearHandlers() {
            _handlers.Clear();
        }

        /// <summary>
        /// アクションの再生
        /// </summary>
        public ActorActionHandle PlayAction(IActorAction action) {
            if (action == null) {
                return ActorActionHandle.Empty;
            }

            if (!_handlers.TryGetValue(action.GetType(), out var handler)) {
                return ActorActionHandle.Empty;
            }

            StopCurrentAction();

            void Finished() {
                FinishedEvent?.Invoke(action, handler.GetOutBlendDuration(action));
            }

            var coroutine = _coroutineRunner.StartCoroutine(handler.PlayRoutine(action));
            _playingInfo = new PlayingInfo(_coroutineRunner, coroutine, action, handler);
            _playingInfo.FinishedEvent += Finished;
            return new ActorActionHandle(_playingInfo);
        }

        /// <summary>
        /// 現在再生中のアクションを停止する
        /// </summary>
        public void StopCurrentAction() {
            _playingInfo?.Exit();
            _playingInfo = null;
        }
    }
}