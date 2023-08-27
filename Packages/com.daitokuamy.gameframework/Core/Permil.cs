using System;
using System.Globalization;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// 千分率計算用
    /// </summary>
    [Serializable]
    public struct Permil {
        // 千分率の1.0
        public const int One = 1000;

        // int型の千分率の素の値
        [SerializeField]
        private int _value;

        #region operators

        public static Permil operator +(Permil a, Permil b) {
            return new Permil(a._value + b._value);
        }

        public static Permil operator +(Permil a, float b) {
            return new Permil(a._value + (int)(b * One));
        }

        public static Permil operator +(float a, Permil b) {
            return new Permil((int)(a * One) + b._value);
        }

        public static Permil operator +(Permil a, int b) {
            return new Permil(a._value + b * One);
        }

        public static Permil operator +(int a, Permil b) {
            return new Permil(a * One + b._value);
        }

        public static Permil operator -(Permil a, Permil b) {
            return new Permil(a._value - b._value);
        }

        public static Permil operator -(Permil a, float b) {
            return new Permil(a._value - (int)(b * One));
        }

        public static Permil operator -(float a, Permil b) {
            return new Permil((int)(a * One) - b._value);
        }

        public static Permil operator -(Permil a, int b) {
            return new Permil(a._value - b * One);
        }

        public static Permil operator -(int a, Permil b) {
            return new Permil(a * One - b._value);
        }

        public static Permil operator *(Permil a, Permil b) {
            return new Permil(a._value * b._value / One);
        }

        public static Permil operator *(Permil a, float b) {
            return new Permil(a._value * (int)(b * One) / One);
        }

        public static Permil operator *(float a, Permil b) {
            return new Permil((int)(a * One) * b._value / One);
        }

        public static Permil operator *(Permil a, int b) {
            return new Permil(a._value * b);
        }

        public static Permil operator *(int a, Permil b) {
            return new Permil(a * b._value);
        }

        public static Permil operator /(Permil a, Permil b) {
            return new Permil(a._value / b._value * One);
        }

        public static Permil operator /(Permil a, float b) {
            return new Permil(a._value / (int)(b * One) * One);
        }

        public static Permil operator /(float a, Permil b) {
            return new Permil((int)(a * One) / b._value * One);
        }

        public static Permil operator /(Permil a, int b) {
            return new Permil(a._value / (b * One) * One);
        }

        public static Permil operator /(int a, Permil b) {
            return new Permil((a * One) / b._value * One);
        }

        public static implicit operator Permil(int value) {
            return new Permil(value * One);
        }

        public static implicit operator int(Permil permil) {
            return permil._value / One;
        }

        public static implicit operator Permil(float value) {
            return new Permil(value);
        }

        public static implicit operator float(Permil permil) {
            return permil._value / (float)One;
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
            _value = permil;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">1.0を基準とした浮動小数値</param>
        public Permil(float value) {
            _value = (int)(value * One);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public override string ToString() {
            return (_value / (float)One).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format) {
            return (_value / (float)One).ToString(format);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(System.IFormatProvider provider) {
            return (_value / (float)One).ToString(provider);
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        public string ToString(string format, System.IFormatProvider provider) {
            return (_value / (float)One).ToString(format, provider);
        }
    }
}