namespace GameFramework.Core {
    /// <summary>
    /// 乱数生成用ユーティリティ
    /// </summary>
    public static class RandomUtil {
        /// <summary>乱数生成用クラス</summary>
        public static readonly FastRandom Random = new(0);
        
        /// <summary>
        /// シード値の設定
        /// </summary>
        /// <param name="seed">シード値</param>
        public static void SetSeed(int seed) {
            Random.SetSeed(seed);
        }

        /// <summary>
        /// 乱数の進行
        /// </summary>
        public static int Next() {
            return Random.Next();
        }

        /// <summary>
        /// 整数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">この値を含まない最大値</param>
        public static int Range(int min, int max) {
            return Random.Range(min, max);
        }

        /// <summary>
        /// 小数型の範囲指定乱数
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        public static float Range(float min, float max) {
            return Random.Range(min, max);
        }
    }
}