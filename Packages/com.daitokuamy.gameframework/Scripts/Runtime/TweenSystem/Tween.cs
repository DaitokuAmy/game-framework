using System;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// Tweenの基底クラス（Tweener/Sequence共通）
    /// </summary>
    public abstract class Tween {
        /// <summary>
        /// Tweenの状態
        /// </summary>
        private enum TweenState {
            /// <summary>未開始</summary>
            Idle,

            /// <summary>再生中</summary>
            Playing,

            /// <summary>一時停止</summary>
            Paused,

            /// <summary>完了</summary>
            Completed,

            /// <summary>Kill</summary>
            Killed,
        }

        private TweenState _state = TweenState.Idle;
        private Action _complateAction;
        private Action _killAction;

        internal bool IsIdleInternal => _state == TweenState.Idle;
        
        /// <summary>完了済みかどうか</summary>
        public bool IsComplete => _state == TweenState.Completed;
        /// <summary>Kill済みかどうか</summary>
        public bool IsKilled => _state == TweenState.Killed;
        /// <summary>一時停止中かどうか</summary>
        public bool IsPaused => _state == TweenState.Paused;
        /// <summary>完了時に自動回収するかどうか（PlayerがPoolに返却）</summary>
        public bool AutoKill { get; private set; } = true;
        
        /// <summary>このTweenの総再生時間</summary>
        public abstract float Duration { get; }

        /// <summary>開始時処理（fromキャプチャなど）</summary>
        protected abstract void OnBegin();

        /// <summary>進行処理</summary>
        protected abstract void OnTick(float deltaTime);

        /// <summary>強制完了時処理（最終状態へ遷移）</summary>
        protected abstract void OnForceComplete();

        /// <summary>完了時処理</summary>
        protected virtual void OnComplete() { }

        /// <summary>Kill時の処理</summary>
        protected virtual void OnKill() { }

        /// <summary>Pause時の処理</summary>
        protected virtual void OnPause() { }

        /// <summary>Resume時の処理</summary>
        protected virtual void OnResume() { }

        /// <summary>Reset時の処理</summary>
        protected virtual void OnReset() { }

        /// <summary>
        /// 完了時に自動回収するかどうかを設定
        /// </summary>
        public Tween SetAutoKill(bool autoKill) {
            AutoKill = autoKill;
            return this;
        }

        /// <summary>
        /// 完了時コールバックを追加
        /// </summary>
        public Tween OnComplete(Action callback) {
            _complateAction += callback;
            return this;
        }

        /// <summary>
        /// Cancel（Kill）時コールバックを追加
        /// </summary>
        public Tween OnCancel(Action callback) {
            _killAction += callback;
            return this;
        }

        /// <summary>
        /// Kill時コールバックを追加
        /// </summary>
        public Tween OnKill(Action callback) {
            _killAction += callback;
            return this;
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        public void Pause() {
            if (_state == TweenState.Playing) {
                _state = TweenState.Paused;
                OnPause();
            }
        }

        /// <summary>
        /// 再開
        /// </summary>
        public void Resume() {
            if (_state == TweenState.Paused) {
                _state = TweenState.Playing;
                OnResume();
            }
        }

        /// <summary>
        /// 強制完了（最終状態まで到達）
        /// </summary>
        public void ForceComplete() {
            if (_state != TweenState.Playing && _state != TweenState.Paused) {
                return;
            }

            _state = TweenState.Playing;
            OnForceComplete();
            CompleteInternal();
        }

        /// <summary>
        /// Kill
        /// </summary>
        public void Kill() {
            if (_state == TweenState.Killed) {
                return;
            }

            _state = TweenState.Killed;
            OnKill();
            _killAction?.Invoke();
        }

        /// <summary>
        /// 再生開始（Playerが呼び出し）
        /// </summary>
        public void BeginInternal() {
            if (_state != TweenState.Idle) {
                return;
            }

            _state = TweenState.Playing;
            OnBegin();
        }

        /// <summary>
        /// Tick更新（Playerが呼び出し）
        /// </summary>
        public void TickInternal(float deltaTime) {
            if (_state != TweenState.Playing) {
                return;
            }

            if (deltaTime <= 0f) {
                return;
            }

            OnTick(deltaTime);
        }

        /// <summary>
        /// Pool返却前に初期状態へ戻す（Playerが呼び出し）
        /// </summary>
        public void ResetInternal() {
            _state = TweenState.Idle;
            AutoKill = true;
            _complateAction = null;
            _killAction = null;
            OnReset();
        }

        /// <summary>
        /// 完了状態へ遷移して完了処理を実行
        /// </summary>
        protected void CompleteInternal() {
            if (_state == TweenState.Completed || _state == TweenState.Killed) {
                return;
            }

            if (_state != TweenState.Playing) {
                return;
            }

            _state = TweenState.Completed;
            OnComplete();
            _complateAction?.Invoke();
        }
    }
}
