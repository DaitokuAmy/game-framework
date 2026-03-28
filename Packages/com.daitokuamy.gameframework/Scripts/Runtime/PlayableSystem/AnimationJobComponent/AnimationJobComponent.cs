using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// Playable提供クラスの基底
    /// </summary>
    public abstract class AnimationJobComponent : IAnimationJobComponent {
        private AnimationScriptPlayable _playable;
        private bool _initialized;
        private bool _disposed;

        // 初期化済みか
        bool IAnimationJobComponent.IsInitialized => _initialized;
        // 廃棄済みか
        bool IAnimationJobComponent.IsDisposed => _disposed;

        /// <inheritdoc/>
        void IAnimationJobComponent.Initialize(Animator animator, PlayableGraph graph) {
            if (_initialized || _disposed) {
                return;
            }

            var playable = CreatePlayable(animator, graph);
            if (!playable.IsValid()) {
                return;
            }

            _playable = playable;
            _playable.SetInputCount(1);
            _initialized = true;
        }

        /// <inheritdoc/>
        void IAnimationJobComponent.Update(float deltaTime) {
            if (!_initialized || !_playable.IsValid()) {
                return;
            }

            UpdateInternal(_playable, deltaTime);
        }

        /// <inheritdoc/>
        AnimationScriptPlayable IAnimationJobComponent.GetPlayable() => _playable;

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            if (_playable.IsValid()) {
                _playable.Destroy();
            }

            DisposeInternal();
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected abstract AnimationScriptPlayable CreatePlayable(Animator animator, PlayableGraph graph);

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="playable">Jobを保持しているPlayable</param>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(AnimationScriptPlayable playable, float deltaTime) {
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }
    }
}
