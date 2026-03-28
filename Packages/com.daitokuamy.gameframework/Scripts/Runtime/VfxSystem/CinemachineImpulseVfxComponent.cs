#if USE_CINEMACHINE

using Unity.Cinemachine;
using UnityEngine;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// CinemachineImpulse制御用のVfxComponent
    /// </summary>
    public sealed class CinemachineImpulseVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("衝撃設定")]
        private CinemachineImpulseSource _impulseSource;
        [SerializeField, Tooltip("遅延時間")]
        private float _delay = 0.0f;

        // 再生中タイマー
        private float _time = 0.0f;
        // 再生中フラグ
        private bool _isPlaying;

        /// <inheritdoc/>
        bool IVfxComponent.IsPlaying => _isPlaying;

        /// <inheritdoc/>
        void IVfxComponent.Tick(float deltaTime) {
            _time += deltaTime;

            if (_time >= 0.0f && !_impulseSource.enabled) {
                _impulseSource.enabled = true;
                _impulseSource.GenerateImpulse();
            }

            if (_time >= _impulseSource.ImpulseDefinition.ImpulseDuration) {
                ((IVfxComponent)this).Stop();
            }
        }

        /// <inheritdoc/>
        void IVfxComponent.Play() {
            if (_impulseSource == null) {
                return;
            }

            _impulseSource.enabled = false;
            _time = -_delay;
            _isPlaying = true;
        }

        /// <inheritdoc/>
        void IVfxComponent.Stop() {
            if (_impulseSource == null) {
                return;
            }
            
            _impulseSource.enabled = false;
            _time = 0.0f;
            _isPlaying = false;
        }

        /// <inheritdoc/>
        void IVfxComponent.StopImmediate() {
            if (_impulseSource == null) {
                return;
            }
            
            _impulseSource.enabled = false;
            _time = 0.0f;
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
            _isPlaying = false;
        }
    }
}

#endif
