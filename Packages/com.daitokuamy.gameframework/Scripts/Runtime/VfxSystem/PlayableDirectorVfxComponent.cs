using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// PlayableDirector制御用のVfxComponent
    /// </summary>
    public sealed class PlayableDirectorVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("再生に使うPlayableDirector")]
        private PlayableDirector _playableDirector;

        // 再生中フラグ
        private bool _isPlaying;

        /// <inheritdoc/>
        bool IVfxComponent.IsPlaying => _isPlaying;
        
        /// <summary>有効なデータか</summary>
        private bool IsValid => _playableDirector != null;

        /// <inheritdoc/>
        void IVfxComponent.Tick(float deltaTime) {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time += deltaTime;
            _playableDirector.Evaluate();
            
            if (_playableDirector.time >= _playableDirector.duration) {
                _isPlaying = false;
            }
        }

        /// <inheritdoc/>
        void IVfxComponent.Play() {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time = 0.0f;
            _isPlaying = true;
        }

        /// <inheritdoc/>
        void IVfxComponent.Stop() {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time = _playableDirector.duration;
            _playableDirector.Evaluate();
            _isPlaying = false;
        }

        /// <inheritdoc/>
        void IVfxComponent.StopImmediate() {
            if (_playableDirector == null) {
                return;
            }

            _playableDirector.time = _playableDirector.duration;
            _playableDirector.Evaluate();
            _isPlaying = false;
        }

        /// <inheritdoc/>
        void IVfxComponent.SetSpeed(float speed) {
        }

        /// <inheritdoc/>
        void IVfxComponent.SetLodLevel(int level) {
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            if (_playableDirector == null) {
                Debug.LogWarning($"Invalid serialize data. : {gameObject.name}");
                return;
            }

            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
            _playableDirector.playOnAwake = false;
            _playableDirector.time = 0.0f;
            _isPlaying = false;
        }
    }
}
