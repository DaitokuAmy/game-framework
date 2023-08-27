using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Playable提供クラスの基底
    /// </summary>
    public abstract class PlayableComponent<TPlayable> : IPlayableComponent
        where TPlayable : IPlayable {
        private Playable _playable;
        private bool _initialized;
        private bool _disposed;
        private bool _autoDispose;

        // 初期化済みか
        bool IPlayableComponent.IsInitialized => _initialized;
        // 廃棄済みか
        bool IPlayableComponent.IsDisposed => _disposed;
        // 自動廃棄フラグ
        bool IPlayableComponent.AutoDispose => _autoDispose;
        
        /// <summary>基礎となるPlayable</summary>
        public abstract TPlayable Playable { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayableComponent(bool autoDispose) {
            _autoDispose = autoDispose;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <inheritdoc/>
        void IPlayableComponent.Initialize(PlayableGraph graph) {
            if (_initialized) {
                return;
            }

            _initialized = true;
            _playable = CreatePlayable(graph);
        }

        /// <summary>
        /// Playableの取得
        /// </summary>
        Playable IPlayableComponent.GetPlayable() {
            return _playable;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void IPlayableComponent.Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 再生時間の設定
        /// </summary>
        void IPlayableComponent.SetTime(float time) {
            _playable.SetTime(time);
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void IPlayableComponent.SetSpeed(float speed) {
            _playable.SetSpeed(speed);
            SetSpeedInternal(speed);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            DisposeInternal();

            if (_playable.IsValid()) {
                _playable.Destroy();
            }
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected abstract Playable CreatePlayable(PlayableGraph graph);
        
        /// <summary>
        /// 速度野設定
        /// </summary>
        protected virtual void SetSpeedInternal(float speed) {}

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void UpdateInternal(float deltaTime) {
        }
    }
}