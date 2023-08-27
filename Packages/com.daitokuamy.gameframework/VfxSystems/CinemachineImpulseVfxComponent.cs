#if USE_CINEMACHINE

using Cinemachine;
using UnityEngine;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// CinemachineImpulse制御用のVfxComponent
    /// </summary>
    public class CinemachineImpulseVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("衝撃設定")]
        private CinemachineImpulseSource _impulseSource;
        [SerializeField, Tooltip("遅延時間")]
        private float _delay = 0.0f;

        // 再生速度
        private float _speed = 1.0f;
        // デフォルトの再生時間
        private float _defaultDuration;
        // 再生中タイマー
        private float _time = 0.0f;
        // 再生中フラグ
        private bool _isPlaying;

        // 再生中か
        bool IVfxComponent.IsPlaying => _isPlaying;

        /// <summary>
        /// 更新処理
        /// </summary>
        void IVfxComponent.Update(float deltaTime) {
            _time += deltaTime;

            if (_time >= 0.0f && !_impulseSource.enabled) {
                _impulseSource.enabled = true;
                _impulseSource.m_ImpulseDefinition.m_ImpulseDuration = _defaultDuration / _speed;
                _impulseSource.GenerateImpulse();
            }

            if (_time >= _defaultDuration) {
                ((IVfxComponent)this).Stop();
            }
        }

        /// <summary>
        /// 再生
        /// </summary>
        void IVfxComponent.Play() {
            if (_impulseSource == null) {
                return;
            }

            _impulseSource.enabled = false;
            _time = -_delay;
            _isPlaying = true;
        }

        /// <summary>
        /// 停止
        /// </summary>
        void IVfxComponent.Stop() {
            if (_impulseSource == null) {
                return;
            }
            
            _impulseSource.enabled = false;
            _time = 0.0f;
            _isPlaying = false;
        }

        /// <summary>
        /// 即時停止
        /// </summary>
        void IVfxComponent.StopImmediate() {
            if (_impulseSource == null) {
                return;
            }
            
            _impulseSource.enabled = false;
            _time = 0.0f;
            _isPlaying = false;
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void IVfxComponent.SetSpeed(float speed) {
            _speed = Mathf.Max(0.001f, speed);
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            if (_impulseSource != null) {
                _defaultDuration = _impulseSource.m_ImpulseDefinition.m_ImpulseDuration;
            }

            _isPlaying = false;
        }
    }
}

#endif