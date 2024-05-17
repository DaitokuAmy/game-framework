using System;
using System.Globalization;
using UnityEngine;

namespace GameFramework.Core {
    /// <summary>
    /// 百分率計算用
    /// </summary>
    [Serializable]
    public struct Percent {
        // 百分率の1.0
        public const int One = 100;

        // int型の百分率の素の値
        [SerializeField]
        private int _value;

        #region operators

        public static Percent operator +(Percent a, Percent b) {
            return new Percent(a._value + b._value);
        }

        public static Percent operator +(Percent a, float b) {
            return new Percent(a._value + (int)(b * One));
        }

        public static Percent operator +(float a, Percent b) {
            return new Percent((int)(a * One) + b._value);
        }

        public static Percent operator +(Percent a, int b) {
            return new Percent(a._value + b * One);
        }

        public static Percent operator +(int a, Percent b) {
            return new Percent(a * One + b._value);
        }

        public static Percent operator -(Percent a, Percent b) {
            return new Percent(a._value - b._value);
        }

        public static Percent operator -(Percent a, float b) {
            return new Percent(a._value - (int)(b * One));
        }

        public static Percent operator -(float a, Percent b) {
            return new Percent((int)(a * One) - b._value);
        }

        public static Percent operator -(Percent a, int b) {
            return new Percent(a._value - b * One);
        }

        public static Percent operator -(int a, Percent b) {
            return new Percent(a * One - b._value);
        }

        public static Percent operator *(Percent a, Percent b) {
            return new Percent(a._value * b._value / One);
        }

        public static Percent operator *(Percent a, float b) {
            return new Percent(a._value * (int)(b * One) / One);
        }

        public static Percent operator *(float a, Percent b) {
            return new Percent((int)(a * One) * b._value / One);
        }

        public static Percent operator *(Percent a, int b) {
            return new Percent(a._value * b);
        }

        public static Percent operator *(int a, Percent b) {
            return new Percent(a * b._value);
        }

        public static Percent operator /(Percent a, Percent b) {
            return new Percent(a._value / b._value * One);
        }

        public static Percent operator /(Percent a, float b) {
            return new Percent(a._value / (int)(b * One) * One);
        }

        public static Percent operator /(float a, Percent b) {
            return new Percent((int)(a * One) / b._value * One);
        }

        public static Percent operator /(Percent a, int b) {
            return new Percent(a._value / (b * One) * One);
        }

        public static Percent operator /(int a, Percent b) {
            return new Percent((a * One) / b._value * One);
        }

        public static implicit operator Percent(int value) {
            return new Percent(value);
        }

        public static implicit operator int(Percent percent) {
            return percent._value;
        }

        public static implicit operator Percent(float value) {
            return new Percent(value);
        }

        public static implicit operator float(Percent percent) {
            return percent._value / (float)One;
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
            _value = percent;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">1.0を基準とした浮動小数値</param>
        public Percent(float value) {
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