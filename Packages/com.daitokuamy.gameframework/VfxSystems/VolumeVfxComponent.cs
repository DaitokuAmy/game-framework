using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework.VfxSystems {
    /// <summary>
    /// Volume制御用のVfxComponent
    /// </summary>
    public class VolumeVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("制御用Volume")]
        private Volume _volume;
        [SerializeField, Tooltip("再生遅延時間")]
        private float _delay = 0.0f;
        [SerializeField, Tooltip("再生時間")]
        private float _duration = 1.0f;

        [SerializeField, Tooltip("Volume反映カーブ")]
        private AnimationCurve _weightCurve;

        // 現在時間
        private float _time;

        // 再生中か
        bool IVfxComponent.IsPlaying => _time < _duration;

        /// <summary>
        /// 更新処理
        /// </summary>
        void IVfxComponent.Update(float deltaTime) {
            if (_volume == null) {
                return;
            }

            _time += deltaTime;
            var rate = Mathf.Clamp01(_time / _duration);
            _volume.weight = _time >= 0.0f ? (_weightCurve != null && _weightCurve.keys.Length > 1 ? _weightCurve.Evaluate(rate) : rate) : 0.0f;
        }

        /// <summary>
        /// 再生
        /// </summary>
        void IVfxComponent.Play() {
            if (_volume == null) {
                return;
            }

            _time = -_delay;
            _volume.enabled = true;
        }

        /// <summary>
        /// 停止
        /// </summary>
        void IVfxComponent.Stop() {
            if (_volume == null) {
                return;
            }

            _time = _duration;
            _volume.weight = 0.0f;
            _volume.enabled = false;
        }

        /// <summary>
        /// 即時停止
        /// </summary>
        void IVfxComponent.StopImmediate() {
            if (_volume == null) {
                return;
            }

            _time = _duration;
            _volume.weight = 0.0f;
            _volume.enabled = false;
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void IVfxComponent.SetSpeed(float speed) {
        }
    }
}