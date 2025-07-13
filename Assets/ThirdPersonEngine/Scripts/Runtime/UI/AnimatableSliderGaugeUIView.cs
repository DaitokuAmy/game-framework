using GameFramework;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace ThirdPersonEngine {
    public class AnimatableSliderGaugeUIView : SliderGaugeUIView {
        [Header("ループアニメーション")]
        [SerializeField, Tooltip("増加時のループアニメーション時間")]
        private float _positiveLoopDuration = 1.0f;
        [SerializeField, Tooltip("減少時のループアニメーション時間")]
        private float _negativeLoopDuration = 1.0f;
        [SerializeField, Tooltip("増加時のループアニメーションルート")]
        private GameObject _positiveLoopRoot;
        [SerializeField, Tooltip("減少時のループアニメーションルート")]
        private GameObject _negativeLoopRoot;
        
        [Header("ワンショットアニメーション")]
        [SerializeField, Tooltip("増加時のワンショットアニメーションルート")]
        private UIAnimationComponent _positiveInvokeAnimation;
        [SerializeField, Tooltip("減少時のワンショットアニメーションルート")]
        private UIAnimationComponent _negativeInvokeAnimation;
        
        private float _positiveTimer;
        private float _negativeTimer;
        private UIAnimationPlayer _animationPlayer;
        private UIAnimationPlayer.Handle _handle;
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal(IScope scope) {
            base.InitializeInternal(scope);
            _animationPlayer = new UIAnimationPlayer().RegisterTo(scope);

            if (_positiveLoopRoot != null) {
                _positiveLoopRoot.SetActive(false);
            }

            if (_negativeLoopRoot != null) {
                _negativeLoopRoot.SetActive(false);
            } 
        }
        
        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            base.LateUpdateInternal(deltaTime);
            
            _animationPlayer.Update(deltaTime);

            if (_positiveTimer > 0.0f) {
                _positiveTimer -= deltaTime;
                
                if (_positiveTimer <= 0.0f) {
                    _positiveLoopRoot.SetActive(false);
                }
            }
            
            if (_negativeTimer > 0.0f) {
                _negativeTimer -= deltaTime;
                
                if (_negativeTimer <= 0.0f) {
                    _negativeLoopRoot.SetActive(false);
                }
            }
        }
        
        protected override void OnStartAnimation(bool negative) {
            base.OnStartAnimation(negative);
            
            _handle.Dispose();

            if (!negative) {
                if (_positiveLoopRoot != null) {
                    _positiveTimer = _positiveLoopDuration;
                    _positiveLoopRoot.SetActive(true);
                }

                if (_positiveInvokeAnimation != null) {
                    _handle = _animationPlayer.Play(_positiveInvokeAnimation);
                }
            }
            
            if (negative) {
                if (_negativeLoopRoot != null) {
                    _negativeTimer = _negativeLoopDuration;
                    _negativeLoopRoot.SetActive(true);
                }
                
                if (_negativeInvokeAnimation != null) {
                    _handle = _animationPlayer.Play(_negativeInvokeAnimation);
                }
            }
        }
    }
}