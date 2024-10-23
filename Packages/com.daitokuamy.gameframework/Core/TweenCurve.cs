using System;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// EasingとCurveを使ったTween機能
    /// </summary>
    [Serializable]
    public class TweenCurve {
        [SerializeField, Tooltip("Easingを使う際のタイプ")]
        private EaseType _easeType;
        [SerializeField, Tooltip("AnimationCurveを使う際のタイプ")]
        private AnimationCurve _animationCurve;
        [SerializeField, Tooltip("AnimationCurveを使うか")]
        private bool _useAnimationCurve;

        /// <summary>
        /// 値の補間
        /// </summary>
        /// <param name="start">開始値</param>
        /// <param name="end">終端値</param>
        /// <param name="ratio">補間割合</param>
        /// <returns>補間結果</returns>
        public float Evaluate(float start, float end, float ratio) {
            if (_useAnimationCurve) {
                return _animationCurve.Evaluate(ratio) * (end - start) + start;
            }

            return _easeType.Evaluate(start, end, ratio);
        }
    }
}