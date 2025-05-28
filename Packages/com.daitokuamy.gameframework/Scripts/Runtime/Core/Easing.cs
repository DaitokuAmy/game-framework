using System;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// イージングタイプ
    /// </summary>
    public enum EaseType {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        Spring,
        Punch
    }

    /// <summary>
    /// 補間計算クラス
    /// </summary>
    public static class Easing {
        // EaseType毎の計算メソッド配列
        private static readonly Func<float, float, float, float>[] Functions = {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuart,
            EaseOutQuart,
            EaseInOutQuart,
            EaseInQuint,
            EaseOutQuint,
            EaseInOutQuint,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc,
            EaseInBounce,
            EaseOutBounce,
            EaseInOutBounce,
            EaseInBack,
            EaseOutBack,
            EaseInOutBack,
            EaseInElastic,
            EaseOutElastic,
            EaseInOutElastic,
            Spring,
            Punch,
        };
        
        /// <summary>
        /// 値の取得
        /// </summary>
        /// <param name="easeType">補間タイプ</param>
        /// <param name="start">開始値</param>
        /// <param name="end">終了値</param>
        /// <param name="ratio">割合</param>
        public static float Evaluate(EaseType easeType, float start, float end, float ratio) {
            return Functions[(int)easeType](start, end, ratio);
        }
        
        /// <summary>
        /// 値の取得
        /// </summary>
        /// <param name="easeType">補間タイプ</param>
        /// <param name="ratio">割合</param>
        public static float Evaluate(EaseType easeType, float ratio) {
            return Functions[(int)easeType](0.0f, 1.0f, ratio);
        }

        private static float EaseInQuad(float start, float end, float ratio) {
            end -= start;
            return end * ratio * ratio + start;
        }

        private static float EaseOutQuad(float start, float end, float ratio) {
            end -= start;
            return -end * ratio * (ratio - 2) + start;
        }

        private static float EaseInOutQuad(float start, float end, float ratio) {
            ratio *= 2;
            end -= start;
            if (ratio < 1) {
                return end / 2 * ratio * ratio + start;
            }

            ratio--;
            return -end / 2 * (ratio * (ratio - 2) - 1) + start;
        }

        private static float EaseInCubic(float start, float end, float ratio) {
            end -= start;
            return end * ratio * ratio * ratio + start;
        }

        private static float EaseOutCubic(float start, float end, float ratio) {
            ratio--;
            end -= start;
            return end * (ratio * ratio * ratio + 1) + start;
        }

        private static float EaseInOutCubic(float start, float end, float ratio) {
            ratio *= 2;
            end -= start;
            if (ratio < 1) {
                return end / 2 * ratio * ratio * ratio + start;
            }

            ratio -= 2;
            return end / 2 * (ratio * ratio * ratio + 2) + start;
        }

        private static float EaseInQuart(float start, float end, float ratio) {
            end -= start;
            return end * ratio * ratio * ratio * ratio + start;
        }

        private static float EaseOutQuart(float start, float end, float ratio) {
            ratio--;
            end -= start;
            return -end * (ratio * ratio * ratio * ratio - 1) + start;
        }

        private static float EaseInOutQuart(float start, float end, float ratio) {
            ratio *= 2;
            end -= start;
            if (ratio < 1) {
                return end / 2 * ratio * ratio * ratio * ratio + start;
            }

            ratio -= 2;
            return -end / 2 * (ratio * ratio * ratio * ratio - 2) + start;
        }

        private static float EaseInQuint(float start, float end, float ratio) {
            end -= start;
            return end * ratio * ratio * ratio * ratio * ratio + start;
        }

        private static float EaseOutQuint(float start, float end, float ratio) {
            ratio--;
            end -= start;
            return end * (ratio * ratio * ratio * ratio * ratio + 1) + start;
        }

        private static float EaseInOutQuint(float start, float end, float ratio) {
            ratio *= 2;
            end -= start;
            if (ratio < 1) return end / 2 * ratio * ratio * ratio * ratio * ratio + start;
            ratio -= 2;
            return end / 2 * (ratio * ratio * ratio * ratio * ratio + 2) + start;
        }

        private static float EaseInSine(float start, float end, float ratio) {
            end -= start;
            return -end * Mathf.Cos(ratio / 1 * (Mathf.PI / 2)) + end + start;
        }

        private static float EaseOutSine(float start, float end, float ratio) {
            end -= start;
            return end * Mathf.Sin(ratio / 1 * (Mathf.PI / 2)) + start;
        }

        private static float EaseInOutSine(float start, float end, float ratio) {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * ratio / 1) - 1) + start;
        }

        private static float EaseInExpo(float start, float end, float ratio) {
            end -= start;
            return end * Mathf.Pow(2, 10 * (ratio / 1 - 1)) + start;
        }

        private static float EaseOutExpo(float start, float end, float ratio) {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * ratio / 1) + 1) + start;
        }

        private static float EaseInOutExpo(float start, float end, float ratio) {
            ratio /= 0.5f;
            end -= start;
            if (ratio < 1) {
                return end / 2 * Mathf.Pow(2, 10 * (ratio - 1)) + start;
            }

            ratio--;
            return end / 2 * (-Mathf.Pow(2, -10 * ratio) + 2) + start;
        }

        private static float EaseInCirc(float start, float end, float ratio) {
            end -= start;
            return -end * (Mathf.Sqrt(1 - ratio * ratio) - 1) + start;
        }

        private static float EaseOutCirc(float start, float end, float ratio) {
            ratio--;
            end -= start;
            return end * Mathf.Sqrt(1 - ratio * ratio) + start;
        }

        private static float EaseInOutCirc(float start, float end, float ratio) {
            ratio /= 0.5f;
            end -= start;
            if (ratio < 1) {
                return -end / 2 * (Mathf.Sqrt(1 - ratio * ratio) - 1) + start;
            }

            ratio -= 2;
            return end / 2 * (Mathf.Sqrt(1 - ratio * ratio) + 1) + start;
        }

        private static float Linear(float start, float end, float ratio) {
            return Mathf.Lerp(start, end, ratio);
        }

        private static float Spring(float start, float end, float ratio) {
            ratio = Mathf.Clamp01(ratio);
            ratio = (Mathf.Sin(ratio * Mathf.PI * (0.2f + 2.5f * ratio * ratio * ratio)) * Mathf.Pow(1f - ratio, 2.2f) + ratio) * (1f + (1.2f * (1f - ratio)));
            return start + (end - start) * ratio;
        }

        private static float EaseInBounce(float start, float end, float ratio) {
            end -= start;
            var d = 1.0f;
            return end - EaseOutBounce(0, end, d - ratio) + start;
        }

        private static float EaseOutBounce(float start, float end, float ratio) {
            ratio /= 1f;
            end -= start;
            if (ratio < (1 / 2.75f)) {
                return end * (7.5625f * ratio * ratio) + start;
            }
            else if (ratio < (2 / 2.75f)) {
                ratio -= (1.5f / 2.75f);
                return end * (7.5625f * (ratio) * ratio + .75f) + start;
            }
            else if (ratio < (2.5 / 2.75)) {
                ratio -= (2.25f / 2.75f);
                return end * (7.5625f * (ratio) * ratio + .9375f) + start;
            }
            else {
                ratio -= (2.625f / 2.75f);
                return end * (7.5625f * (ratio) * ratio + .984375f) + start;
            }
        }

        private static float EaseInOutBounce(float start, float end, float ratio) {
            end -= start;
            var d = 1.0f;
            if (ratio < d / 2) {
                return EaseInBounce(0, end, ratio * 2) * 0.5f + start;
            }
            else {
                return EaseOutBounce(0, end, ratio * 2 - d) * 0.5f + end * 0.5f + start;
            }
        }

        private static float EaseInBack(float start, float end, float ratio) {
            end -= start;
            ratio /= 1;
            var s = 1.70158f;
            return end * (ratio) * ratio * ((s + 1) * ratio - s) + start;
        }

        private static float EaseOutBack(float start, float end, float ratio) {
            var s = 1.70158f;
            end -= start;
            ratio = (ratio / 1) - 1;
            return end * ((ratio) * ratio * ((s + 1) * ratio + s) + 1) + start;
        }

        private static float EaseInOutBack(float start, float end, float ratio) {
            var s = 1.70158f;
            end -= start;
            ratio /= .5f;
            if ((ratio) < 1) {
                s *= (1.525f);
                return end / 2 * (ratio * ratio * (((s) + 1) * ratio - s)) + start;
            }

            ratio -= 2;
            s *= (1.525f);
            return end / 2 * ((ratio) * ratio * (((s) + 1) * ratio + s) + 2) + start;
        }

        private static float EaseInElastic(float start, float end, float ratio) {
            end -= start;

            var d = 1.0f;
            var p = d * 0.3f;
            var s = 0.0f;
            var a = 0.0f;

            if (ratio == 0) {
                return start;
            }

            if (Mathf.Abs((ratio /= d) - 1) < float.Epsilon) {
                return start + end;
            }

            if (a == 0f || a < Mathf.Abs(end)) {
                a = end;
                s = p / 4;
            }
            else {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return -(a * Mathf.Pow(2, 10 * (ratio -= 1)) * Mathf.Sin((ratio * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        private static float EaseOutElastic(float start, float end, float ratio) {
            end -= start;

            var d = 1.0f;
            var p = d * 0.3f;
            var s = 0.0f;
            var a = 0.0f;

            if (ratio == 0) {
                return start;
            }

            if (Mathf.Abs((ratio /= d) - 1) < float.Epsilon) {
                return start + end;
            }

            if (a == 0f || a < Mathf.Abs(end)) {
                a = end;
                s = p / 4;
            }
            else {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * ratio) * Mathf.Sin((ratio * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        private static float EaseInOutElastic(float start, float end, float ratio) {
            end -= start;

            var d = 1.0f;
            var p = d * 0.3f;
            var s = 0.0f;
            var a = 0.0f;

            if (ratio == 0) {
                return start;
            }

            if (Mathf.Abs((ratio /= d / 2) - 2) < float.Epsilon) {
                return start + end;
            }

            if (a == 0f || a < Mathf.Abs(end)) {
                a = end;
                s = p / 4;
            }
            else {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (ratio < 1) {
                return -0.5f * (a * Mathf.Pow(2, 10 * (ratio -= 1)) * Mathf.Sin((ratio * d - s) * (2 * Mathf.PI) / p)) + start;
            }

            return a * Mathf.Pow(2, -10 * (ratio -= 1)) * Mathf.Sin((ratio * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }

        private static float Punch(float start, float end, float ratio) {
            return end;
        }
    }
}