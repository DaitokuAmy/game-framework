using System;
using System.Globalization;

namespace GameFramework.Core {
    /// <summary>
    /// 千分率計算用
    /// </summary>
    [Serializable]
    public struct Permil : IEquatable<Permil>, IComparable<Permil> {
        /// <summary>千分率の1.0</summary>
        public const int One = 1000;

        /// <summary>permil値</summary>
        public int Value;

        #region operators

        public static bool operator ==(Permil a, Permil b) {
            return a.Value == b.Value;
        }

        public static bool operator ==(Permil a, float b) {
            return a.Value == (int)(b * One);
        }

        public static bool operator ==(float a, Permil b) {
            return (int)(a * One) == b.Value;
        }

        public static bool operator !=(Permil a, Permil b) {
            return !(a == b);
        }

        public static bool operator !=(Permil a, float b) {
            return !(a == b);
        }

        public static bool operator !=(float a, Permil b) {
            return !(a == b);
        }

        public static bool operator <(Permil a, Permil b) {
            return a.Value < b.Value;
        }

        public static bool operator <(Permil a, float b) {
            return a.Value < b * One;
        }

        public static bool operator <(float a, Permil b) {
            return a * One < b.Value;
        }

        public static bool operator >(Permil a, Permil b) {
            return a.Value > b.Value;
        }

        public static bool operator >(Permil a, float b) {
            return a.Value > b * One;
        }

        public static bool operator >(float a, Permil b) {
            return a * One > b.Value;
        }

        public static bool operator <=(Permil a, Permil b) {
            return !(a > b);
        }

        public static bool operator <=(Permil a, float b) {
            return !(a > b);
        }

        public static bool operator <=(float a, Permil b) {
            return !(a > b);
        }

        public static bool operator >=(Permil a, Permil b) {
            return !(a < b);
        }

        public static bool operator >=(Permil a, float b) {
            return !(a < b);
        }

        public static bool operator >=(float a, Permil b) {
            return !(a < b);
        }

        public static Permil operator +(Permil a, Permil b) {
            return new Permil(a.Value + b.Value);
        }

        public static Permil operator +(Permil a, float b) {
            return new Permil(a.Value + (int)(b * One));
        }

        public static Permil operator +(float a, Permil b) {
            return new Permil((int)(a * One) + b.Value);
        }

        public static Permil operator +(Permil a, int b) {
            return new Permil(a.Value + b * One);
        }

        public static Permil operator +(int a, Permil b) {
            return new Permil(a * One + b.Value);
        }

        public static Permil operator -(Permil a, Permil b) {
            return new Permil(a.Value - b.Value);
        }

        public static Permil operator -(Permil a, float b) {
            return new Permil(a.Value - (int)(b * One));
        }

        public static Permil operator -(float a, Permil b) {
            return new Permil((int)(a * One) - b.Value);
        }

        public static Permil operator -(Permil a, int b) {
            return new Permil(a.Value - b * One);
        }

        public static Permil operator -(int a, Permil b) {
            return new Permil(a * One - b.Value);
        }

        public static Permil operator *(Permil a, Permil b) {
            return new Permil(a.Value * b.Value / One);
        }

        public static Permil operator *(Permil a, float b) {
            return new Permil(a.Value * (int)(b * One) / One);
        }

        public static Permil operator *(float a, Permil b) {
            return new Permil((int)(a * One) * b.Value / One);
        }

        public static Permil operator *(Permil a, int b) {
            return new Permil(a.Value * b);
        }

        public static Permil operator *(int a, Permil b) {
            return new Permil(a * b.Value);
        }

        public static Permil operator /(Permil a, Permil b) {
            return new Permil(a.Value / b.Value * One);
        }

        public static Permil operator /(Permil a, float b) {
            return new Permil(a.Value / (int)(b * One) * One);
        }

        public static Permil operator /(float a, Permil b) {
            return new Permil((int)(a * One) / b.Value * One);
        }

        public static Permil operator /(Permil a, int b) {
            return new Permil(a.Value / (b * One) * One);
        }

        public static Permil operator /(int a, Permil b) {
            return new Permil((a * One) / b.Value * One);
        }

        public static explicit operator int(Permil permil) {
            return permil.Value / One;
        }

        public static implicit operator Permil(int value) {
            return new Permil(value);
        }

        public static implicit operator Permil(float value) {
            return new Permil(value);
        }

        public static implicit operator float(Permil permil) {
            return permil.Value / (float)One;
        }

        #endregion

        /// <summary>
        /// 整数値(permilではない)を元にPermilを作る
        /// </summary>
        /// <param name="value">Permilではないただのint値</param>
        public static Permil CreateFromIntValue(int value) {
            return new Permil(value * One);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="permil">千分率の値(1000を1.0とした物)</param>
        public Permil(int permil) {
            Value = permil;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">1.0を基準とした浮動小数値</param>
        public Permil(float value) {
            Value = (int)(value * One);
        }

        /// <summary>
        /// 比較
        /// </summary>
        public override bool Equals(object obj) {
            if (obj is not Permil permil) {
                return false;
            }

            return Value == permil.Value;
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
        public bool Equals(Permil other) {
            return Value == other.Value;
        }

        /// <summary>
        /// 比較
        /// </summary>
        public int CompareTo(Permil other) {
            return Value.CompareTo(other.Value);
        }
    }
}