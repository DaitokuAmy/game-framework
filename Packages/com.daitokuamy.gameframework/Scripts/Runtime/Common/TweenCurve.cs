using System;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// EasingとCurveを使ったTween機能
    /// </summary>
    [Serializable]
    public class TweenCurve {
        [SerializeField, Tooltip("Easingを使う際のタイプ")]
        private EaseType _easeType;
        [SerializeField, Tooltip("AnimationCurveを使う際のタイプ")]
        private AnimationCurve _animationCurve = new(new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0));
        [SerializeField, Tooltip("AnimationCurveを使うか")]
        private bool _useAnimationCurve;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TweenCurve(EaseType easeType) {
            _useAnimationCurve = false;
            _easeType = easeType;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TweenCurve(AnimationCurve animationCurve) {
            _useAnimationCurve = true;
            _animationCurve = animationCurve;
        }

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