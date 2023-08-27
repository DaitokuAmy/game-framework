namespace GameFramework.Core {
    /// <summary>
    /// 乱数生成用ユーティリティ
    /// </summary>
    public static class RandomUtil {
        private static readonly FastRandom s_random = new FastRandom(0);
        
        /// <summary>
        /// シード値の設定
        /// </summary>
        /// <param name="seed">シード値</param>
        public static void SetSeed(int seed) {
            s_random.SetSeed(seed);
        }

        /// <summary>
        /// 乱数の進行
        /// </summary>
        public static int Next() {
            return s_random.Next();
        }

        /// <summary>
        /// 整数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">この値を含まない最大値</param>
        public static int Range(int min, int max) {
            return s_random.Range(min, max);
        }

        /// <summary>
        /// 小数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        public static float Range(float min, float max) {
            return s_random.Range(min, max);
        }
    }
}