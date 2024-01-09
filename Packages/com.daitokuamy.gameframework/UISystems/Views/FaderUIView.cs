using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// フェード制御用のUIView
    /// </summary>
    public abstract class FaderUIView : UIView {
        private float _currentRate;
        private float _targetRate;
        private float _animationTimer;
        private AsyncOperator _fadeOperator;
        
        /// <summary>
        /// フェードイン
        /// </summary>
        /// <param name="duration">フェードイン時間</param>
        public AsyncOperationHandle FadeInAsync(float duration) {
            _targetRate = 0.0f;
            _animationTimer = duration;
            if (_fadeOperator != null) {
                _fadeOperator.Aborted();
                _fadeOperator = null;
            }

            _fadeOperator = new AsyncOperator();
            return _fadeOperator;
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        /// <param name="color">フェードアウト色</param>
        /// <param name="duration">フェードアウト時間</param>
        public AsyncOperationHandle FadeOutAsync(Color color, float duration) {
            _targetRate = 1.0f;
            _animationTimer = duration;
            SetColor(color);
            if (_fadeOperator != null) {
                _fadeOperator.Aborted();
                _fadeOperator = null;
            }

            _fadeOperator = new AsyncOperator();
            return _fadeOperator;
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            base.LateUpdateInternal(deltaTime);
            
            // 更新
            if (_animationTimer >= 0.0f) {
                _currentRate = Mathf.Lerp(_currentRate, _targetRate, deltaTime >= _animationTimer ? 1.0f : deltaTime / _animationTimer);
                _animationTimer -= deltaTime;
                ApplyRate(_currentRate);

                if (_animationTimer < 0.0f) {
                    if (_fadeOperator != null) {
                        _fadeOperator.Completed();
                        _fadeOperator = null;
                    }
                }
            }
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        protected abstract void SetColor(Color color);

        /// <summary>
        /// 割合の反映
        /// </summary>
        protected abstract void ApplyRate(float rate);
    }
}
