using System;
using System.Globalization;

namespace GameFramework.Core {
    /// <summary>
    /// 百分率計算用
    /// </summary>
    [Serializable]
    public struct Percent : IEquatable<Percent>, IComparable<Percent> {
        /// <summary>百分率の1.0</summary>
        public const int One = 100;

        /// <summary>percent値</summary>
        public int Value;

        #region operators

        public static bool operator ==(Percent a, Percent b) {
            return a.Value == b.Value;
        }

        public static bool operator ==(Percent a, float b) {
            return a.Value == (int)(b * One);
        }

        public static bool operator ==(float a, Percent b) {
            return (int)(a * One) == b.Value;
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
            return a.Value < b.Value;
        }

        public static bool operator <(Percent a, float b) {
            return a.Value < b * One;
        }

        public static bool operator <(float a, Percent b) {
            return a * One < b.Value;
        }

        public static bool operator >(Percent a, Percent b) {
            return a.Value > b.Value;
        }

        public static bool operator >(Percent a, float b) {
            return a.Value > b * One;
        }

        public static bool operator >(float a, Percent b) {
            return a * One > b.Value;
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
            return new Percent(a.Value + b.Value);
        }

        public static Percent operator +(Percent a, float b) {
            return new Percent(a.Value + (int)(b * One));
        }

        public static Percent operator +(float a, Percent b) {
            return new Percent((int)(a * One) + b.Value);
        }

        public static Percent operator +(Percent a, int b) {
            return new Percent(a.Value + b * One);
        }

        public static Percent operator +(int a, Percent b) {
            return new Percent(a * One + b.Value);
        }

        public static Percent operator -(Percent a, Percent b) {
            return new Percent(a.Value - b.Value);
        }

        public static Percent operator -(Percent a, float b) {
            return new Percent(a.Value - (int)(b * One));
        }

        public static Percent operator -(float a, Percent b) {
            return new Percent((int)(a * One) - b.Value);
        }

        public static Percent operator -(Percent a, int b) {
            return new Percent(a.Value - b * One);
        }

        public static Percent operator -(int a, Percent b) {
            return new Percent(a * One - b.Value);
        }

        public static Percent operator *(Percent a, Percent b) {
            return new Percent(a.Value * b.Value / One);
        }

        public static Percent operator *(Percent a, float b) {
            return new Percent(a.Value * (int)(b * One) / One);
        }

        public static Percent operator *(float a, Percent b) {
            return new Percent((int)(a * One) * b.Value / One);
        }

        public static Percent operator *(Percent a, int b) {
            return new Percent(a.Value * b);
        }

        public static Percent operator *(int a, Percent b) {
            return new Percent(a * b.Value);
        }

        public static Percent operator /(Percent a, Percent b) {
            return new Percent(a.Value / b.Value * One);
        }

        public static Percent operator /(Percent a, float b) {
            return new Percent(a.Value / (int)(b * One) * One);
        }

        public static Percent operator /(float a, Percent b) {
            return new Percent((int)(a * One) / b.Value * One);
        }

        public static Percent operator /(Percent a, int b) {
            return new Percent(a.Value / (b * One) * One);
        }

        public static Percent operator /(int a, Percent b) {
            return new Percent((a * One) / b.Value * One);
        }

        public static explicit operator int(Percent percent) {
            return percent.Value / One;
        }

        public static implicit operator Percent(int value) {
            return new Percent(value);
        }

        public static implicit operator Percent(float value) {
            return new Percent(value);
        }

        public static implicit operator float(Percent percent) {
            return percent.Value / (float)One;
        }

        #endregion

        /// <summary>
        /// 整数値(percentではない)を元にPercentを作る
        /// </summary>
        /// <param name="value">Percentではないただのint値</param>
        public static Percent CreateFromIntValue(int value) {
            return new Percent(value * One);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="percent">百分率の値(100を1.0とした物)</param>
        public Percent(int percent) {
            Value = percent;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">1.0を基準とした浮動小数値</param>
        public Percent(float value) {
            Value = (int)(value * One);
        }

        /// <summary>
        /// 比較
        /// </summary>
        public override bool Equals(object obj) {
            if (obj is not Percent percent) {
                return false;
            }

            return Value == percent.Value;
        }

        /// <summary>
        /// ハッシュコードの生成
        /// </summary>
        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public override string ToString() {
            return (Value / (float)One).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format) {
            return (Value / (float)One).ToString(format);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(IFormatProvider provider) {
            return (Value / (float)One).ToString(provider);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format, IFormatProvider provider) {
            return (Value / (float)One).ToString(format, provider);
        }

        /// <summary>
        /// 比較
        /// </summary>
        public bool Equals(Percent other) {
            return Value == other.Value;
        }

        /// <summary>
        /// 比較
        /// </summary>
        public int CompareTo(Percent other) {
            return Value.CompareTo(other.Value);
        }
    }
}