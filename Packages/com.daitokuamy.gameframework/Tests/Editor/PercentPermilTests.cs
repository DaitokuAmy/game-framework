using NUnit.Framework;
using GameFramework.Core;
using System.Globalization;

namespace GameFramework.Tests {
    /// <summary>
    /// Percent と Permil の動作検証テスト
    /// </summary>
    public class PercentPermilTests {
        /// <summary>
        /// 浮動小数点の誤差を吸収して比較するアサーション
        /// </summary>
        void AssertAlmostEqual(float expected, float actual, float tolerance = 0.001f) {
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance));
        }

        #region Percent Tests

        /// <summary>
        /// Percent のコンストラクタと基本プロパティが正しく動作するかを検証する
        /// </summary>
        [Test]
        public void Percent_Constructors_And_Properties_Work() {
            var percent = new Percent(0.75f);
            Assert.AreEqual(75, percent.RawValue);
            AssertAlmostEqual(0.75f, percent.AsFloat);
            Assert.IsFalse(percent.IsZero);
            Assert.IsFalse(percent.IsOne);
        }

        /// <summary>
        /// Percent の等価比較・大小比較演算子の挙動を検証する
        /// </summary>
        [Test]
        public void Percent_Equality_And_Comparison_Work() {
            var a = new Percent(1.0f);
            var b = new Percent(1.0f);
            var c = new Percent(0.5f);

            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
            Assert.IsTrue(a > c);
            Assert.IsTrue(c < a);
            Assert.IsTrue(a >= b);
            Assert.IsTrue(c <= a);
        }

        /// <summary>
        /// Percent の加減乗除演算が正しく行われるかを検証する
        /// </summary>
        [Test]
        public void Percent_ArithmeticOperations_Work() {
            var a = new Percent(0.6f);
            var b = new Percent(0.4f);

            Assert.AreEqual(new Percent(1.0f), a + b);
            Assert.AreEqual(new Percent(0.2f), a - b);
            Assert.AreEqual(new Percent(0.24f), a * b);
            AssertAlmostEqual(1.5f, a / b);
        }

        /// <summary>
        /// Percent のキャストが正しく行われるかを検証する
        /// </summary>
        [Test]
        public void Percent_Casting_Works() {
            Percent fromFloat = 0.5f;
            Percent fromInt = 200;

            Assert.AreEqual(50, fromFloat.RawValue);
            Assert.AreEqual(200, fromInt.RawValue);

            float toFloat = fromFloat;
            int toInt = (int)fromInt;

            AssertAlmostEqual(0.5f, toFloat);
            Assert.AreEqual(2, toInt);
        }

        /// <summary>
        /// Percent の Clamp メソッドと Lerp メソッドの挙動を検証する
        /// </summary>
        [Test]
        public void Percent_ClampAndLerp_Work() {
            var min = new Percent(0.2f);
            var max = new Percent(0.8f);
            var val = new Percent(1.0f);

            Assert.AreEqual(max, val.Clamp(min, max));

            var lerped = Percent.Lerp(min, max, 0.5f);
            Assert.AreEqual(new Percent(0.5f), lerped);
        }

        /// <summary>
        /// Percent の ToString メソッドが正しい文字列を出力するかを検証する
        /// </summary>
        [Test]
        public void Percent_ToString_Work() {
            var value = new Percent(0.25f);
            Assert.AreEqual("0.25", value.ToString());
            Assert.AreEqual("25 %", value.AsFloat.ToString("P0", CultureInfo.InvariantCulture));
        }

        #endregion

        #region Permil Tests

        /// <summary>
        /// Permil のコンストラクタと基本プロパティが正しく動作するかを検証する
        /// </summary>
        [Test]
        public void Permil_Constructors_And_Properties_Work() {
            var permil = new Permil(0.75f);
            Assert.AreEqual(750, permil.RawValue);
            AssertAlmostEqual(0.75f, permil.AsFloat);
            Assert.IsFalse(permil.IsZero);
            Assert.IsFalse(permil.IsOne);
        }

        /// <summary>
        /// Permil の等価比較・大小比較演算子の挙動を検証する
        /// </summary>
        [Test]
        public void Permil_Equality_And_Comparison_Work() {
            var a = new Permil(1.0f);
            var b = new Permil(1.0f);
            var c = new Permil(0.25f);

            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
            Assert.IsTrue(a > c);
            Assert.IsTrue(c < a);
            Assert.IsTrue(a >= b);
            Assert.IsTrue(c <= a);
        }

        /// <summary>
        /// Permil の加減乗除演算が正しく行われるかを検証する
        /// </summary>
        [Test]
        public void Permil_ArithmeticOperations_Work() {
            var a = new Permil(0.6f);
            var b = new Permil(0.4f);

            Assert.AreEqual(new Permil(1.0f), a + b);
            Assert.AreEqual(new Permil(0.2f), a - b);
            Assert.AreEqual(new Permil(0.24f), a * b);
            AssertAlmostEqual(1.5f, a / b);
        }

        /// <summary>
        /// Permil のキャストが正しく行われるかを検証する
        /// </summary>
        [Test]
        public void Permil_Casting_Works() {
            Permil fromFloat = 0.5f;
            Permil fromInt = 2000;

            Assert.AreEqual(500, fromFloat.RawValue);
            Assert.AreEqual(2000, fromInt.RawValue);

            float toFloat = fromFloat;
            int toInt = (int)fromInt;

            AssertAlmostEqual(0.5f, toFloat);
            Assert.AreEqual(2, toInt);
        }

        /// <summary>
        /// Permil の Clamp メソッドと Lerp メソッドの挙動を検証する
        /// </summary>
        [Test]
        public void Permil_ClampAndLerp_Work() {
            var min = new Permil(0.2f);
            var max = new Permil(0.8f);
            var val = new Permil(1.0f);

            Assert.AreEqual(max, val.Clamp(min, max));

            var lerped = Permil.Lerp(min, max, 0.5f);
            Assert.AreEqual(new Permil(0.5f), lerped);
        }

        /// <summary>
        /// Permil の ToString メソッドが正しい文字列を出力するかを検証する
        /// </summary>
        [Test]
        public void Permil_ToString_Work() {
            var value = new Permil(0.125f);
            Assert.AreEqual("0.125", value.ToString());
            Assert.AreEqual("13 %", value.AsFloat.ToString("P0", CultureInfo.InvariantCulture));
        }

        #endregion
    }
}