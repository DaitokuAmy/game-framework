using UnityEngine;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// Active制御用のVfxComponent
    /// </summary>
    public sealed class ActiveVfxComponent : MonoBehaviour, IVfxComponent {
        [SerializeField, Tooltip("制御用GameObject")]
        private GameObject[] _targetObjects;
        [SerializeField, Tooltip("表示遅延時間")]
        private float _delay = 0.0f;
        [SerializeField, Tooltip("表示時間")]
        private float _duration = 1.0f;
        [SerializeField, Tooltip("ループか")]
        private bool _loop;

        // 現在時間
        private float _time;

        /// <inheritdoc/>
        bool IVfxComponent.IsPlaying => _time < _duration;

        /// <inheritdoc/>
        void IVfxComponent.Tick(float deltaTime) {
            if (_targetObjects.Length <= 0.0f) {
                return;
            }

            _time += deltaTime;
            var active = _time >= 0.0f && (_loop || _time < _duration);
            SetActive(active);
        }

        /// <inheritdoc/>
        void IVfxComponent.Play() {
            if (_targetObjects.Length <= 0.0f) {
                return;
            }

            _time = -_delay;
            SetActive(_time >= 0.0f);
        }

        /// <inheritdoc/>
        void IVfxComponent.Stop() {
            if (_targetObjects.Length <= 0.0f) {
                return;
            }

            _time = _duration;
            SetActive(false);
        }

        /// <inheritdoc/>
        void IVfxComponent.StopImmediate() {
            if (_targetObjects.Length <= 0.0f) {
                return;
            }

            _time = _duration;
            SetActive(false);
        }

        /// <inheritdoc/>
        void IVfxComponent.SetSpeed(float speed) {
        }

        /// <inheritdoc/>
        void IVfxComponent.SetLodLevel(int level) {
        }

        /// <summary>
        /// 対象のGameObjectのアクティブ状態を指定
        /// </summary>
        private void SetActive(bool active) {
            foreach (var obj in _targetObjects) {
                if (obj == null) {
                    continue;
                }
                
                if (obj.activeSelf != active) {
                    obj.SetActive(active);
                }
            }
        }
    }
}