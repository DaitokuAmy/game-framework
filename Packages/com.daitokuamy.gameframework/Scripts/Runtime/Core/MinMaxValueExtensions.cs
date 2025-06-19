namespace GameFramework.Core {
    /// <summary>
    /// MinMaxValue用の拡張メソッド
    /// </summary>
    public static class MinMaxValueExtensions {
        /// <summary>
        /// ランダムに値を求める
        /// </summary>
        public static T Rand<T>(this IMinMaxValue<T> source, int seed) {
            if (source.UseRandom) {
                var random = new FastRandom(seed);
                return source.Rand(random);
            }

            return source.MinValue;
        }

        /// <summary>
        /// ランダムに値を求める
        /// </summary>
        public static T Rand<T>(this IMinMaxValue<T> source) {
            if (source.UseRandom) {
                return source.Rand(RandomUtil.Random);
            }

            return source.MinValue;
        }

        /// <summary>
        /// ランダムに値を求める
        /// </summary>
        public static float RandEvaluate(this MinMaxAnimationCurve source, float time, int seed) {
            if (source.useRandom) {
                var random = new FastRandom(seed);
                return source.LerpEvaluate(time, random.Range(0.0f, 1.0f));
            }

            return source.minValue.Evaluate(time);
        }

        /// <summary>
        /// ランダムに値を求める
        /// </summary>
        public static float RandEvaluate(this MinMaxAnimationCurve source, float time) {
            if (source.useRandom) {
                return source.LerpEvaluate(time, RandomUtil.Range(0.0f, 1.0f));
            }

            return source.minValue.Evaluate(time);
        }
    }
}