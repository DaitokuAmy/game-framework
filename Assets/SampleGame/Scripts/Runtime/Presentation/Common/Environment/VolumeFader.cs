using UnityEngine;
using UnityEngine.Rendering;

namespace SampleGame.Presentation {
    /// <summary>
    /// VolumeのWeightコントロール用フェーダー
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class VolumeFader : MonoBehaviour {
        private Volume _volume;
        private float _targetWeight;
        private float _timer;

        /// <summary>タイムスケール</summary>
        public float TimeScale { get; set; } = 1.0f;

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _volume = GetComponent<Volume>();
            _volume.weight = _targetWeight;
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            var deltaTime = Time.deltaTime * TimeScale;

            if (_timer >= 0.0f) {
                _timer -= deltaTime;
                var ratio = _timer > float.Epsilon ? deltaTime / _timer : 1.0f;
                _volume.weight = Mathf.Lerp(_volume.weight, _targetWeight, ratio);
            }
        }

        /// <summary>
        /// Weightの設定
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="duration"></param>
        public void Fade(float weight, float duration) {
            _targetWeight = weight;
            _timer = duration;
        }
    }
}