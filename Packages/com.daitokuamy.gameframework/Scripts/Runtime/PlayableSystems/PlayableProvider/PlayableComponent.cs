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

        /// <inheritdoc/>
        bool IPlayableComponent.IsInitialized => _initialized;
        /// <inheritdoc/>
        bool IPlayableComponent.IsDisposed => _disposed;
        
        /// <summary>基礎となるPlayable</summary>
        public abstract TPlayable Playable { get; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        void IPlayableComponent.Initialize(PlayableGraph graph) {
            if (_initialized) {
                return;
            }

            _initialized = true;
            _playable = CreatePlayable(graph);
        }

        /// <inheritdoc/>
        Playable IPlayableComponent.GetPlayable() {
            return _playable;
        }

        /// <inheritdoc/>
        void IPlayableComponent.Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }

        /// <inheritdoc/>
        void IPlayableComponent.SetTime(float time) {
            _playable.SetTime(time);
        }

        /// <inheritdoc/>
        void IPlayableComponent.SetSpeed(float speed) {
            SetSpeedInternal(speed);
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