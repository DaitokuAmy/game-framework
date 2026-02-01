using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameFramework.VfxSystem {
    /// <summary>
    /// Volume制御用のVfxComponent
    /// </summary>
    public sealed class LoopVolumeVfxComponent : MonoBehaviour, IVfxComponent {
        // フェードタイプ
        private enum FadeType {
            None,
            FadeIn,
            FadeOut,
        }

        // フェード情報
        [Serializable]
        private class FadeInfo {
            [Tooltip("フェード時間")]
            public float duration;
            [Tooltip("フェードカーブ")]
            public AnimationCurve weightCurve;
        }

        [SerializeField, Tooltip("制御用Volume")]
        private Volume _volume;
        [SerializeField, Tooltip("フェードイン情報")]
        private FadeInfo _fadeIn;
        [SerializeField, Tooltip("フェードアウト情報")]
        private FadeInfo _fadeOut;

        private FadeType _fadeType;
        private float _fadeTime;
        private float _fadeStartWeight;

        /// <inheritdoc/>
        bool IVfxComponent.IsPlaying => _volume != null && _volume.enabled;

        /// <inheritdoc/>
        void IVfxComponent.Tick(float deltaTime) {
            if (_volume == null) {
                return;
            }

            // Animation
            var info = _fadeType == FadeType.FadeIn ? _fadeIn : (_fadeType == FadeType.FadeOut ? _fadeOut : null);
            if (info != null) {
                // 時間更新
                _fadeTime += deltaTime;

                // VolumeのWeight更新
                var rate = info.duration > float.Epsilon ? Mathf.Clamp01(_fadeTime / info.duration) : 1.0f;
                rate = info.weightCurve != null && info.weightCurve.keys.Length > 1
                    ? info.weightCurve.Evaluate(rate)
                    : rate;
                var targetWeight = _fadeType == FadeType.FadeIn ? 1.0f : 0.0f;
                _volume.weight = Mathf.Lerp(_fadeStartWeight, targetWeight, rate);

                // アニメーション完了
                if (rate >= 1.0f) {
                    if (_fadeType == FadeType.FadeOut) {
                        _volume.enabled = false;
                    }

                    _fadeType = FadeType.None;
                }
            }
        }

        /// <inheritdoc/>
        void IVfxComponent.Play() {
            if (_volume == null) {
                return;
            }

            _fadeType = FadeType.FadeIn;
            _fadeTime = 0.0f;
            _volume.enabled = true;
            _volume.weight = 0.0f;
            _fadeStartWeight = 0.0f;
        }

        /// <inheritdoc/>
        void IVfxComponent.Stop() {
            if (_volume == null) {
                return;
            }

            _fadeType = FadeType.FadeOut;
            _fadeTime = 0.0f;
            _fadeStartWeight = _volume.weight;
        }

        /// <inheritdoc/>
        void IVfxComponent.StopImmediate() {
            if (_volume == null) {
                return;
            }

            _fadeType = FadeType.None;
            _fadeTime = 0.0f;
            _volume.weight = 0.0f;
            _volume.enabled = false;
        }

        /// <inheritdoc/>
        void IVfxComponent.SetSpeed(float speed) {
        }

        /// <inheritdoc/>
        void IVfxComponent.SetLodLevel(int level) {
        }
    }
}