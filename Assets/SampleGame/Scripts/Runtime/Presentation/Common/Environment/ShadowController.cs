using GameFramework.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SampleGame.Presentation {
    /// <summary>
    /// 影制御様コンポーネント
    /// </summary>
    public class ShadowController : MonoBehaviour {
        private float _defaultMaxDistance;
        private float _targetDistance;
        private float _currentDistance;
        private float _maxDistanceTimer = -1.0f;

        private LayeredTime _layeredTime;

        /// <summary>
        /// 時間管理クラスの設定
        /// </summary>
        public void SetLayeredTime(LayeredTime layeredTime) {
            _layeredTime = layeredTime;
        }

        /// <summary>
        /// 影距離の変更
        /// </summary>
        public void ChangeDistance(float distance, float duration = 0.0f) {
            _targetDistance = distance;
            _maxDistanceTimer = duration;
        }

        /// <summary>
        /// アクティブ処理
        /// </summary>
        private void OnEnable() {
            var currentAsset = QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel()) as UniversalRenderPipelineAsset;
            if (currentAsset != null) {
                _defaultMaxDistance = currentAsset.shadowDistance;
                _currentDistance = _defaultMaxDistance;
                _targetDistance = _defaultMaxDistance;
            }

            _maxDistanceTimer = -1.0f;

            QualitySettings.activeQualityLevelChanged += OnChangedQualityLevel;
        }

        /// <summary>
        /// 非アクティブ処理
        /// </summary>
        private void OnDisable() {
            QualitySettings.activeQualityLevelChanged -= OnChangedQualityLevel;

            var currentAsset = QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel()) as UniversalRenderPipelineAsset;
            if (currentAsset != null) {
                currentAsset.shadowDistance = _defaultMaxDistance;
            }

            _maxDistanceTimer = -1.0f;
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        private void LateUpdate() {
            var deltaTime = _layeredTime != null ? _layeredTime.DeltaTime : Time.deltaTime;
            UpdateShadowDistance(deltaTime);
        }

        /// <summary>
        /// クォリティ設定の変更通知
        /// </summary>
        private void OnChangedQualityLevel(int prevLevel, int currentLevel) {
            var prevAsset = QualitySettings.GetRenderPipelineAssetAt(prevLevel) as UniversalRenderPipelineAsset;
            var currentAsset = QualitySettings.GetRenderPipelineAssetAt(currentLevel) as UniversalRenderPipelineAsset;

            if (prevAsset != null) {
                prevAsset.shadowDistance = _defaultMaxDistance;
            }

            if (currentAsset != null) {
                _defaultMaxDistance = currentAsset.shadowDistance;
                currentAsset.shadowDistance = _currentDistance;
            }
        }

        /// <summary>
        /// 影距離の更新
        /// </summary>
        private void UpdateShadowDistance(float deltaTime) {
            if (deltaTime <= float.Epsilon) {
                return;
            }

            if (_maxDistanceTimer < 0.0f) {
                return;
            }

            var rate = _maxDistanceTimer > float.Epsilon ? Mathf.Clamp01(deltaTime / _maxDistanceTimer) : 1.0f;
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, rate);

            var currentAsset = QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel()) as UniversalRenderPipelineAsset;
            if (currentAsset != null) {
                currentAsset.shadowDistance = _currentDistance;
            }
        }
    }
}