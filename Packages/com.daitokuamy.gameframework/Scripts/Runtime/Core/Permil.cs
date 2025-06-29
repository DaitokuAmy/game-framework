using System;
using System.Globalization;

namespace GameFramework.Core {
    /// <summary>
    /// 千分率計算用
    /// </summary>
    [Serializable]
    public struct Permil : IEquatable<Permil>, IComparable<Permil>, IFormattable {
        /// <summary>千分率の1.0</summary>
        public const int UnitValue = 1000;
        /// <summary>0‰を表す定数</summary>
        public static readonly Permil Zero = new(0);
        /// <summary>1000‰を表す定数</summary>
        public static readonly Permil One = new(UnitValue);

        /// <summary>生値</summary>
        public int RawValue;
        /// <summary>小数点値</summary>
        public float AsFloat => RawValue / (float)UnitValue;
        /// <summary>千分率値</summary>
        public int AsPermil => RawValue;
        /// <summary>Zeroか</summary>
        public bool IsZero => RawValue == 0;
        /// <summary>1000‰か</summary>
        public bool IsOne => RawValue == UnitValue;

        #region operators

        public static bool operator ==(Permil a, Permil b) {
            return a.RawValue == b.RawValue;
        }

        public static bool operator ==(Permil a, float b) {
            return a.RawValue == FloatMath.RoundToInt(b * UnitValue);
        }

        public static bool operator ==(float a, Permil b) {
            return FloatMath.RoundToInt(a * UnitValue) == b.RawValue;
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
            return a.RawValue < b.RawValue;
        }

        public static bool operator <(Permil a, float b) {
            return a.RawValue < b * UnitValue;
        }

        public static bool operator <(float a, Permil b) {
            return a * UnitValue < b.RawValue;
        }

        public static bool operator >(Permil a, Permil b) {
            return a.RawValue > b.RawValue;
        }

        public static bool operator >(Permil a, float b) {
            return a.RawValue > b * UnitValue;
        }

        public static bool operator >(float a, Permil b) {
            return a * UnitValue > b.RawValue;
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
            return new Permil(a.RawValue + b.RawValue);
        }

        public static Permil operator +(Permil a, float b) {
            return new Permil(a.RawValue + FloatMath.RoundToInt(b * UnitValue));
        }

        public static Permil operator +(float a, Permil b) {
            return new Permil(FloatMath.RoundToInt(a * UnitValue) + b.RawValue);
        }

        public static Permil operator +(Permil a, int b) {
            return new Permil(a.RawValue + b * UnitValue);
        }

        public static Permil operator +(int a, Permil b) {
            return new Permil(a * UnitValue + b.RawValue);
        }

        public static Permil operator -(Permil a, Permil b) {
            return new Permil(a.RawValue - b.RawValue);
        }

        public static Permil operator -(Permil a, float b) {
            return new Permil(a.RawValue - FloatMath.RoundToInt(b * UnitValue));
        }

        public static Permil operator -(float a, Permil b) {
            return new Permil(FloatMath.RoundToInt(a * UnitValue) - b.RawValue);
        }

        public static Permil operator -(Permil a, int b) {
            return new Permil(a.RawValue - b * UnitValue);
        }

        public static Permil operator -(int a, Permil b) {
            return new Permil(a * UnitValue - b.RawValue);
        }

        public static Permil operator *(Permil a, Permil b) {
            return new Permil(a.RawValue * b.RawValue / UnitValue);
        }

        public static Permil operator *(Permil a, float b) {
            return new Permil(a.RawValue * FloatMath.RoundToInt(b * UnitValue) / UnitValue);
        }

        public static Permil operator *(float a, Permil b) {
            return new Permil(FloatMath.RoundToInt(a * UnitValue) * b.RawValue / UnitValue);
        }

        public static Permil operator *(Permil a, int b) {
            return new Permil(a.RawValue * b);
        }

        public static Permil operator *(int a, Permil b) {
            return new Permil(a * b.RawValue);
        }

        public static float operator /(Permil a, Permil b) {
            return a.RawValue / (float)b.RawValue;
        }

        public static Permil operator /(Permil a, float b) {
            return new Permil(a.RawValue / FloatMath.RoundToInt(b * UnitValue) * UnitValue);
        }

        public static float operator /(float a, Permil b) {
            return a * UnitValue / b.RawValue;
        }

        public static Permil operator /(Permil a, int b) {
            return new Permil(a.RawValue / (b * UnitValue) * UnitValue);
        }

        public static float operator /(int a, Permil b) {
            return a * UnitValue / (float)b.RawValue;
        }

        public static explicit operator int(Permil permil) {
            return permil.RawValue / UnitValue;
        }

        public static implicit operator Permil(int value) {
            return new Permil(value);
        }

        public static implicit operator Permil(float value) {
            return new Permil(value);
        }

        public static implicit operator float(Permil permil) {
            return permil.RawValue / (float)UnitValue;
        }

        #endregion

        /// <summary>
        /// 整数値(permilではない)を元にPermilを作る
        /// </summary>
        /// <param name="value">Permilではないただのint値</param>
        public static Permil CreateFromIntValue(int value) {
            return new Permil(value * UnitValue);
        }

        /// <summary>
        /// 線形補間
        /// </summary>
        public static Permil Lerp(Permil a, Permil b, float t) {
            return new Permil(a.AsFloat + (b.AsFloat - a.AsFloat) * t);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="permil">千分率の値(1000を1.0とした物)</param>
        public Permil(int permil) {
            RawValue = permil;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">1.0を基準とした浮動小数値</param>
        public Permil(float value) {
            RawValue = FloatMath.RoundToInt(value * UnitValue);
        }

        /// <summary>
        /// 比較
        /// </summary>
        public override bool Equals(object obj) {
            if (obj is not Permil permil) {
                return false;
            }

            return RawValue == permil.RawValue;
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
        public bool Equals(Permil other) {
            return RawValue == other.RawValue;
        }

        /// <summary>
        /// 比較
        /// </summary>
        public int CompareTo(Permil other) {
            return RawValue.CompareTo(other.RawValue);
        }

        /// <summary>
        /// 値のクランプ
        /// </summary>
        public Permil Clamp(Permil min, Permil max) {
            if (this < min) return min;
            if (this > max) return max;
            return this;
        }
    }
}