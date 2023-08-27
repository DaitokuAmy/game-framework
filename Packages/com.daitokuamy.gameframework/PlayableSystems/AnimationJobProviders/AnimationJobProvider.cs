using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Playable提供クラスの基底
    /// </summary>
    public abstract class AnimationJobProvider : IAnimationJobProvider {
        private AnimationScriptPlayable _playable;
        private bool _initialized;
        private bool _disposed;

        // 初期化済みか
        bool IAnimationJobProvider.IsInitialized => _initialized;
        // 廃棄済みか
        bool IAnimationJobProvider.IsDisposed => _disposed;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <inheritdoc/>
        void IAnimationJobProvider.Initialize(Animator animator, PlayableGraph graph) {
            if (_initialized) {
                return;
            }

            _initialized = true;
            _playable = CreatePlayable(animator, graph);
            _playable.SetInputCount(1);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void IAnimationJobProvider.Update(float deltaTime) {
            UpdateInternal(_playable, deltaTime);
        }

        /// <summary>
        /// Playableの取得
        /// </summary>
        AnimationScriptPlayable IAnimationJobProvider.GetPlayable() => _playable;

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