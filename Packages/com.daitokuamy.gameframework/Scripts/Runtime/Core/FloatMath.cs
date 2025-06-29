using System;

namespace GameFramework.Core {
    /// <summary>
    /// float型用の数学関数
    /// </summary>
    public static class FloatMath {
        /// <summary>円周率 (π)</summary>
        public const float PI = (float)Math.PI;
        /// <summary>度からラジアンへの変換係数</summary>
        public const float Deg2Rad = PI / 180.0f;
        /// <summary>ラジアンから度への変換係数</summary>
        public const float Rad2Deg = 180.0f / PI;

        /// <summary>
        /// 値の絶対値を返します。
        /// </summary>
        /// <param name="value">対象の数値</param>
        public static float Abs(float value) {
            return Math.Abs(value);
        }

        /// <summary>
        /// 2つの値のうち小さい方を返します。
        /// </summary>
        /// <param name="a">1つ目の値</param>
        /// <param name="b">2つ目の値</param>
        public static float Min(float a, float b) {
            return a < b ? a : b;
        }

        /// <summary>
        /// 2つの値のうち大きい方を返します。
        /// </summary>
        /// <param name="a">1つ目の値</param>
        /// <param name="b">2つ目の値</param>
        public static float Max(float a, float b) {
            return a > b ? a : b;
        }

        /// <summary>
        /// 値を指定範囲内に制限します。
        /// </summary>
        /// <param name="value">対象の値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        public static float Clamp(float value, float min, float max) {
            return Max(min, Min(value, max));
        }

        /// <summary>
        /// 値を0.0から1.0の範囲に制限します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Clamp01(float value) {
            return Clamp(value, 0.0f, 1.0f);
        }

        /// <summary>
        /// 線形補間を行います（範囲制限あり）。
        /// </summary>
        /// <param name="a">開始値</param>
        /// <param name="b">終了値</param>
        /// <param name="t">補間係数（0.0～1.0）</param>
        public static float Lerp(float a, float b, float t) {
            return a + (b - a) * Clamp01(t);
        }

        /// <summary>
        /// 線形補間を行います（範囲制限なし）。
        /// </summary>
        /// <param name="a">開始値</param>
        /// <param name="b">終了値</param>
        /// <param name="t">補間係数</param>
        public static float LerpUnclamped(float a, float b, float t) {
            return a + (b - a) * t;
        }

        /// <summary>
        /// 指定された最大変化量内で現在値を目標値に近づけます。
        /// </summary>
        /// <param name="current">現在値</param>
        /// <param name="target">目標値</param>
        /// <param name="maxDelta">最大変化量</param>
        public static float MoveTowards(float current, float target, float maxDelta) {
            if (Math.Abs(target - current) <= maxDelta) {
                return target;
            }

            return current + Math.Sign(target - current) * maxDelta;
        }

        /// <summary>
        /// 平方根を計算します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Sqrt(float value) {
            return (float)Math.Sqrt(value);
        }

        /// <summary>
        /// 累乗を計算します。
        /// </summary>
        /// <param name="baseValue">底</param>
        /// <param name="exponent">指数</param>
        public static float Pow(float baseValue, float exponent) {
            return (float)Math.Pow(baseValue, exponent);
        }

        /// <summary>
        /// 指数関数（eのx乗）を返します。
        /// </summary>
        /// <param name="power">指数</param>
        public static float Exp(float power) {
            return (float)Math.Exp(power);
        }

        /// <summary>
        /// 自然対数を返します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Log(float value) {
            return (float)Math.Log(value);
        }

        /// <summary>
        /// 常用対数（底10）を返します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Log10(float value) {
            return (float)Math.Log10(value);
        }

        /// <summary>
        /// サイン（正弦）を計算します。
        /// </summary>
        /// <param name="angleRad">ラジアン角</param>
        public static float Sin(float angleRad) {
            return (float)Math.Sin(angleRad);
        }

        /// <summary>
        /// コサイン（余弦）を計算します。
        /// </summary>
        /// <param name="angleRad">ラジアン角</param>
        public static float Cos(float angleRad) {
            return (float)Math.Cos(angleRad);
        }

        /// <summary>
        /// タンジェント（正接）を計算します。
        /// </summary>
        /// <param name="angleRad">ラジアン角</param>
        public static float Tan(float angleRad) {
            return (float)Math.Tan(angleRad);
        }

        /// <summary>
        /// アークサイン（逆正弦）を計算します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Asin(float value) {
            return (float)Math.Asin(value);
        }

        /// <summary>
        /// アークコサイン（逆余弦）を計算します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Acos(float value) {
            return (float)Math.Acos(value);
        }

        /// <summary>
        /// アークタンジェント（逆正接）を計算します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Atan(float value) {
            return (float)Math.Atan(value);
        }

        /// <summary>
        /// 2次元ベクトルのアークタンジェントを計算します。
        /// </summary>
        /// <param name="y">y成分</param>
        /// <param name="x">x成分</param>
        public static float Atan2(float y, float x) {
            return (float)Math.Atan2(y, x);
        }

        /// <summary>
        /// 値を0からlengthの範囲にラップします。
        /// </summary>
        /// <param name="t">対象の値</param>
        /// <param name="length">周期</param>
        public static float Repeat(float t, float length) {
            return t - Floor(t / length) * length;
        }

        /// <summary>
        /// 値をPingPong波形として変換します。
        /// </summary>
        /// <param name="t">対象の値</param>
        /// <param name="length">最大値</param>
        public static float PingPong(float t, float length) {
            t = Repeat(t, length * 2.0f);
            return length - Math.Abs(t - length);
        }

        /// <summary>
        /// 小数点以下を切り捨てます。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Floor(float value) {
            return (float)Math.Floor(value);
        }

        /// <summary>
        /// 小数点以下を切り上げます。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Ceil(float value) {
            return (float)Math.Ceiling(value);
        }

        /// <summary>
        /// 四捨五入します。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static float Round(float value) {
            return (float)Math.Round(value);
        }

        /// <summary>
        /// 四捨五入して int に変換します。
        /// </summary>
        /// <param name="value">変換する float 値</param>
        public static int RoundToInt(float value) {
            return (int)Math.Round(value);
        }

        /// <summary>
        /// 値の符号を返します（0以上なら1、負なら-1）。
        /// </summary>
        /// <param name="value">対象の値</param>
        public static int Sign(float value) {
            return value < 0.0f ? -1 : 1;
        }

        /// <summary>
        /// 角度の差を最小回転方向（-180～180）で返します。
        /// </summary>
        /// <param name="current">現在角度</param>
        /// <param name="target">目標角度</param>
        public static float DeltaAngle(float current, float target) {
            float delta = Repeat((target - current) + 180.0f, 360.0f) - 180.0f;
            return delta;
        }

        /// <summary>
        /// 2つの float 値がほぼ等しいかどうかを比較します。
        /// </summary>
        /// <param name="a">1つ目の値</param>
        /// <param name="b">2つ目の値</param>
        /// <param name="epsilon">許容誤差（省略時は 1e-6）</param>
        public static bool Approximately(float a, float b, float epsilon = 1e-6f) {
            return Math.Abs(a - b) < epsilon;
        }
    }
}