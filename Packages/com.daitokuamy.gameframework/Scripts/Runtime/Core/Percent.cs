using System;
using System.Globalization;

namespace GameFramework.Core {
    /// <summary>
    /// 百分率計算用
    /// </summary>
    [Serializable]
    public struct Percent : IEquatable<Percent>, IComparable<Percent>, IFormattable {
        /// <summary>百分率の1.0</summary>
        public const int UnitValue = 100;
        /// <summary>0%を表す定数</summary>
        public static readonly Percent Zero = new(0);
        /// <summary>100%を表す定数</summary>
        public static readonly Percent One = new(UnitValue);

        /// <summary>生値</summary>
        public int RawValue;
        /// <summary>小数点値</summary>
        public float AsFloat => RawValue / (float)UnitValue;
        /// <summary>百分率値</summary>
        public int AsPercent => RawValue;
        /// <summary>Zeroか</summary>
        public bool IsZero => RawValue == 0;
        /// <summary>100%か</summary>
        public bool IsOne => RawValue == UnitValue;

        #region operators

        public static bool operator ==(Percent a, Percent b) {
            return a.RawValue == b.RawValue;
        }

        public static bool operator ==(Percent a, float b) {
            return a.RawValue == FloatMath.RoundToInt(b * UnitValue);
        }

        public static bool operator ==(float a, Percent b) {
            return FloatMath.RoundToInt(a * UnitValue) == b.RawValue;
        }

        public static bool operator !=(Percent a, Percent b) {
            return !(a == b);
        }

        public static bool operator !=(Percent a, float b) {
            return !(a == b);
        }

        public static bool operator !=(float a, Percent b) {
            return !(a == b);
        }

        public static bool operator <(Percent a, Percent b) {
            return a.RawValue < b.RawValue;
        }

        public static bool operator <(Percent a, float b) {
            return a.RawValue < b * UnitValue;
        }

        public static bool operator <(float a, Percent b) {
            return a * UnitValue < b.RawValue;
        }

        public static bool operator >(Percent a, Percent b) {
            return a.RawValue > b.RawValue;
        }

        public static bool operator >(Percent a, float b) {
            return a.RawValue > b * UnitValue;
        }

        public static bool operator >(float a, Percent b) {
            return a * UnitValue > b.RawValue;
        }

        public static bool operator <=(Percent a, Percent b) {
            return !(a > b);
        }

        public static bool operator <=(Percent a, float b) {
            return !(a > b);
        }

        public static bool operator <=(float a, Percent b) {
            return !(a > b);
        }

        public static bool operator >=(Percent a, Percent b) {
            return !(a < b);
        }

        public static bool operator >=(Percent a, float b) {
            return !(a < b);
        }

        public static bool operator >=(float a, Percent b) {
            return !(a < b);
        }

        public static Percent operator +(Percent a, Percent b) {
            return new Percent(a.RawValue + b.RawValue);
        }

        public static Percent operator +(Percent a, float b) {
            return new Percent(a.RawValue + FloatMath.RoundToInt(b * UnitValue));
        }

        public static Percent operator +(float a, Percent b) {
            return new Percent(FloatMath.RoundToInt(a * UnitValue) + b.RawValue);
        }

        public static Percent operator +(Percent a, int b) {
            return new Percent(a.RawValue + b * UnitValue);
        }

        public static Percent operator +(int a, Percent b) {
            return new Percent(a * UnitValue + b.RawValue);
        }

        public static Percent operator -(Percent a, Percent b) {
            return new Percent(a.RawValue - b.RawValue);
        }

        public static Percent operator -(Percent a, float b) {
            return new Percent(a.RawValue - FloatMath.RoundToInt(b * UnitValue));
        }

        public static Percent operator -(float a, Percent b) {
            return new Percent(FloatMath.RoundToInt(a * UnitValue) - b.RawValue);
        }

        public static Percent operator -(Percent a, int b) {
            return new Percent(a.RawValue - b * UnitValue);
        }

        public static Percent operator -(int a, Percent b) {
            return new Percent(a * UnitValue - b.RawValue);
        }

        public static Percent operator *(Percent a, Percent b) {
            return new Percent(a.RawValue * b.RawValue / UnitValue);
        }

        public static Percent operator *(Percent a, float b) {
            return new Percent(a.RawValue * FloatMath.RoundToInt(b * UnitValue) / UnitValue);
        }

        public static Percent operator *(float a, Percent b) {
            return new Percent(FloatMath.RoundToInt(a * UnitValue) * b.RawValue / UnitValue);
        }

        public static Percent operator *(Percent a, int b) {
            return new Percent(a.RawValue * b);
        }

        public static Percent operator *(int a, Percent b) {
            return new Percent(a * b.RawValue);
        }

        public static float operator /(Percent a, Percent b) {
            return a.RawValue / (float)b.RawValue;
        }

        public static Percent operator /(Percent a, float b) {
            return new Percent(a.RawValue / FloatMath.RoundToInt(b * UnitValue) * UnitValue);
        }

        public static float operator /(float a, Percent b) {
            return a * UnitValue / b.RawValue;
        }

        public static Percent operator /(Percent a, int b) {
            return new Percent(a.RawValue / (b * UnitValue) * UnitValue);
        }

        public static float operator /(int a, Percent b) {
            return a * UnitValue / (float)b.RawValue;
        }

        public static explicit operator int(Percent percent) {
            return percent.RawValue / UnitValue;
        }

        public static implicit operator Percent(int value) {
            return new Percent(value);
        }

        public static implicit operator Percent(float value) {
            return new Percent(value);
        }

        public static implicit operator float(Percent percent) {
            return percent.RawValue / (float)UnitValue;
        }

        #endregion

        /// <summary>
        /// 整数値(percentではない)を元にPercentを作る
        /// </summary>
        /// <param name="value">Percentではないただのint値</param>
        public static Percent CreateFromIntValue(int value) {
            return new Percent(value * UnitValue);
        }

        /// <summary>
        /// 線形補間
        /// </summary>
        public static Percent Lerp(Percent a, Percent b, float t) {
            return new Percent(a.AsFloat + (b.AsFloat - a.AsFloat) * t);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="percent">百分率の値(100を1.0とした物)</param>
        public Percent(int percent) {
            RawValue = percent;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">1.0を基準とした浮動小数値</param>
        public Percent(float value) {
            RawValue = FloatMath.RoundToInt(value * UnitValue);
        }

        /// <summary>
        /// 比較
        /// </summary>
        public override bool Equals(object obj) {
            if (obj is not Percent percent) {
                return false;
            }

            return RawValue == percent.RawValue;
        }

        /// <summary>
        /// ハッシュコードの生成
        /// </summary>
        public override int GetHashCode() {
            return RawValue.GetHashCode();
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public override string ToString() {
            return (RawValue / (float)UnitValue).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format) {
            return (RawValue / (float)UnitValue).ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(IFormatProvider provider) {
            return (RawValue / (float)UnitValue).ToString(provider);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format, IFormatProvider provider) {
            return (RawValue / (float)UnitValue).ToString(format, provider);
        }

        /// <summary>
        /// 比較
        /// </summary>
        public bool Equals(Percent other) {
            return RawValue == other.RawValue;
        }

        /// <summary>
        /// 比較
        /// </summary>
        public int CompareTo(Percent other) {
            return RawValue.CompareTo(other.RawValue);
        }

        /// <summary>
        /// 値のクランプ
        /// </summary>
        public Percent Clamp(Percent min, Percent max) {
            if (this < min) return min;
            if (this > max) return max;
            return this;
        }
    }
}