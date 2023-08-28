using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {    
    /// <summary>
    /// CanvasGroupを操作するシンプルなUIAnimation
    /// </summary>
    public class CanvasGroupUIAnimationComponent : UIAnimationComponent {
        [Header("Target")]
        [SerializeField, Tooltip("制御対象のCanvasGropu")]
        private CanvasGroup _canvasGroup;
        
        [Header("Animation")]
        [SerializeField, Tooltip("開始α値")]
        private float _startAlpha = 0.0f;
        [SerializeField, Tooltip("終了α値")]
        private float _exitAlpha = 1.0f;
        [SerializeField, Tooltip("α値の補間タイプ")]
        private EaseType _easeTypeAlpha = EaseType.Linear;
        [SerializeField, Tooltip("開始スケール値")]
        private float _startScale = 1.0f;
        [SerializeField, Tooltip("終了スケール値")]
        private float _exitScale = 1.0f;
        [SerializeField, Tooltip("スケールの補間タイプ")]
        private EaseType _easeTypeScale = EaseType.Linear;
        [SerializeField, Tooltip("再生時間")]
        private float _duration = 1.0f;

        /// <summary>トータル時間</summary>
        public override float Duration => _duration;
        
        /// <summary>
        /// 時間の設定
        /// </summary>
        /// <param name="time">現在時間</param>
        protected override void SetTimeInternal(float time) {
            if (_canvasGroup != null) {
                var ratio = Duration > float.Epsilon ? time / Duration : 1.0f;
                _canvasGroup.alpha = _easeTypeAlpha.Evaluate(_startAlpha, _exitAlpha, ratio);
                _canvasGroup.transform.localScale = _easeTypeScale.Evaluate(_startScale, _exitScale, ratio) * Vector3.one;
            }
        }
    }
}