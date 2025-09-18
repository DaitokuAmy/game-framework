using System;

namespace GameFramework.Core {
    /// <summary>
    /// int型用の数学関数
    /// </summary>
    public static class IntMath {
        /// <summary>
        /// 値の絶対値を返します
        /// </summary>
        /// <param name="value">対象の数値</param>
        public static int Abs(int value) {
            return Math.Abs(value);
        }

        /// <summary>
        /// 2つの値のうち小さい方を返します
        /// </summary>
        /// <param name="a">1つ目の値</param>
        /// <param name="b">2つ目の値</param>
        public static int Min(int a, int b) {
            return a < b ? a : b;
        }

        /// <summary>
        /// 2つの値のうち大きい方を返します
        /// </summary>
        /// <param name="a">1つ目の値</param>
        /// <param name="b">2つ目の値</param>
        public static int Max(int a, int b) {
            return a > b ? a : b;
        }

        /// <summary>
        /// 値を指定範囲内に制限します
        /// </summary>
        /// <param name="value">対象の値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        public static int Clamp(int value, int min, int max) {
            return Max(min, Min(value, max));
        }

        /// <summary>
        /// 二乗を計算します
        /// </summary>
        /// <param name="baseValue">底</param>
        public static int Pow2(int baseValue) {
            return baseValue * baseValue;
        }

        /// <summary>
        /// 値を0からlengthの範囲にリピートします
        /// </summary>
        /// <param name="t">対象の値</param>
        /// <param name="length">周期</param>
        public static int Repeat(int t, int length) {
            return t >= 0 ? t % length : (length + t % length) % length;
        }

        /// <summary>
        /// 値をPingPong波形として変換します
        /// </summary>
        /// <param name="t">対象の値</param>
        /// <param name="length">最大値</param>
        public static int PingPong(int t, int length) {
            t = Repeat(t, length * 2);
            return length - Math.Abs(t - length);
        }
    }
}