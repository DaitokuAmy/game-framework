using NUnit.Framework;
using GameFramework.Core;

namespace GameFramework.Tests {
    /// <summary>
    /// GameFramework.Core.FloatMath クラスの完全網羅ユニットテスト
    /// </summary>
    public class FloatMathTests {
        /// <summary>
        /// Abs 関数が絶対値を返すことを確認します。
        /// </summary>
        [TestCase(5.0f, ExpectedResult = 5.0f)]
        [TestCase(-3.0f, ExpectedResult = 3.0f)]
        public float Abs_ReturnsAbsoluteValue(float input) {
            return FloatMath.Abs(input);
        }

        /// <summary>
        /// Min 関数が小さい方の値を返すことを確認します。
        /// </summary>
        [TestCase(2.0f, 5.0f, ExpectedResult = 2.0f)]
        [TestCase(10.0f, -3.0f, ExpectedResult = -3.0f)]
        public float Min_ReturnsSmallerValue(float a, float b) {
            return FloatMath.Min(a, b);
        }

        /// <summary>
        /// Max 関数が大きい方の値を返すことを確認します。
        /// </summary>
        [TestCase(2.0f, 5.0f, ExpectedResult = 5.0f)]
        [TestCase(10.0f, -3.0f, ExpectedResult = 10.0f)]
        public float Max_ReturnsLargerValue(float a, float b) {
            return FloatMath.Max(a, b);
        }

        /// <summary>
        /// Clamp 関数が値を指定範囲に制限することを確認します。
        /// </summary>
        [TestCase(5.0f, 0.0f, 10.0f, ExpectedResult = 5.0f)]
        [TestCase(-2.0f, 0.0f, 10.0f, ExpectedResult = 0.0f)]
        [TestCase(15.0f, 0.0f, 10.0f, ExpectedResult = 10.0f)]
        public float Clamp_ClampsValue(float value, float min, float max) {
            return FloatMath.Clamp(value, min, max);
        }

        /// <summary>
        /// Clamp01 関数が値を 0.0～1.0 に制限することを確認します。
        /// </summary>
        [TestCase(0.5f, ExpectedResult = 0.5f)]
        [TestCase(-1.0f, ExpectedResult = 0.0f)]
        [TestCase(1.5f, ExpectedResult = 1.0f)]
        public float Clamp01_ClampsValueTo01(float value) {
            return FloatMath.Clamp01(value);
        }

        /// <summary>
        /// Lerp 関数が補間値を計算することを確認します。
        /// </summary>
        [TestCase(0.0f, 10.0f, 0.5f, ExpectedResult = 5.0f)]
        [TestCase(-5.0f, 5.0f, 0.25f, ExpectedResult = -2.5f)]
        public float Lerp_Interpolates(float a, float b, float t) {
            return FloatMath.Lerp(a, b, t);
        }

        /// <summary>
        /// LerpUnclamped 関数が範囲外の補間値を返すことを確認します。
        /// </summary>
        [TestCase(0.0f, 10.0f, 1.5f, ExpectedResult = 15.0f)]
        public float LerpUnclamped_InterpolatesWithoutClamp(float a, float b, float t) {
            return FloatMath.LerpUnclamped(a, b, t);
        }

        /// <summary>
        /// MoveTowards 関数が値を目標に近づけることを確認します。
        /// </summary>
        [TestCase(5.0f, 6.0f, 2.0f, ExpectedResult = 6.0f)]
        [TestCase(0.0f, 10.0f, 3.0f, ExpectedResult = 3.0f)]
        public float MoveTowards_MovesTowardsTarget(float current, float target, float maxDelta) {
            return FloatMath.MoveTowards(current, target, maxDelta);
        }

        /// <summary>
        /// Sqrt 関数が平方根を返すことを確認します。
        /// </summary>
        [TestCase(9.0f, ExpectedResult = 3.0f)]
        public float Sqrt_ReturnsCorrectRoot(float value) {
            return FloatMath.Sqrt(value);
        }

        /// <summary>
        /// Pow 関数が累乗を計算することを確認します。
        /// </summary>
        [TestCase(2.0f, 3.0f, ExpectedResult = 8.0f)]
        public float Pow_ReturnsPower(float baseValue, float exponent) {
            return FloatMath.Pow(baseValue, exponent);
        }

        /// <summary>
        /// Exp 関数が指数関数を返すことを確認します。
        /// </summary>
        [TestCase(1.0f, ExpectedResult = 2.7182817f, Description = "Check with delta")]
        public float Exp_ReturnsExponential(float value) {
            return FloatMath.Exp(value);
        }

        /// <summary>
        /// Log 関数が自然対数を返すことを確認します。
        /// </summary>
        [TestCase(2.7182817f)]
        public void Log_ReturnsNaturalLog(float value) {
            Assert.That(FloatMath.Log(value), Is.EqualTo(1.0f).Within(0.0001f));
        }

        /// <summary>
        /// Log10 関数が常用対数を返すことを確認します。
        /// </summary>
        [TestCase(100.0f, ExpectedResult = 2.0f)]
        public float Log10_ReturnsBase10Log(float value) {
            return FloatMath.Log10(value);
        }

        /// <summary>
        /// Sin 関数が正弦を返すことを確認します。
        /// </summary>
        [TestCase(0.0f, ExpectedResult = 0.0f)]
        public float Sin_ReturnsSine(float angleRad) {
            return FloatMath.Sin(angleRad);
        }

        /// <summary>
        /// Cos 関数が余弦を返すことを確認します。
        /// </summary>
        [TestCase(0.0f, ExpectedResult = 1.0f)]
        public float Cos_ReturnsCosine(float angleRad) {
            return FloatMath.Cos(angleRad);
        }

        /// <summary>
        /// Tan 関数が正接を返すことを確認します。
        /// </summary>
        [TestCase(0.0f, ExpectedResult = 0.0f)]
        public float Tan_ReturnsTangent(float angleRad) {
            return FloatMath.Tan(angleRad);
        }

        /// <summary>
        /// Asin 関数が逆正弦を返すことを確認します。
        /// </summary>
        [TestCase(0.0f, ExpectedResult = 0.0f)]
        public float Asin_ReturnsArcSine(float value) {
            return FloatMath.Asin(value);
        }

        /// <summary>
        /// Acos 関数が逆余弦を返すことを確認します。
        /// </summary>
        [TestCase(1.0f, ExpectedResult = 0.0f)]
        public float Acos_ReturnsArcCosine(float value) {
            return FloatMath.Acos(value);
        }

        /// <summary>
        /// Atan 関数が逆正接を返すことを確認します。
        /// </summary>
        [TestCase(0.0f, ExpectedResult = 0.0f)]
        public float Atan_ReturnsArcTangent(float value) {
            return FloatMath.Atan(value);
        }

        /// <summary>
        /// Atan2 関数が座標から角度を返すことを確認します。
        /// </summary>
        [TestCase(1.0f, 1.0f, ExpectedResult = 0.7853982f)]
        public float Atan2_ReturnsCorrectAngle(float y, float x) {
            return FloatMath.Atan2(y, x);
        }

        /// <summary>
        /// Repeat 関数が値をループさせることを確認します。
        /// </summary>
        [TestCase(7.0f, 5.0f, ExpectedResult = 2.0f)]
        public float Repeat_LoopsCorrectly(float t, float length) {
            return FloatMath.Repeat(t, length);
        }

        /// <summary>
        /// PingPong 関数が反射値を返すことを確認します。
        /// </summary>
        [TestCase(7.0f, 5.0f, ExpectedResult = 3.0f)]
        public float PingPong_ReflectsCorrectly(float t, float length) {
            return FloatMath.PingPong(t, length);
        }

        /// <summary>
        /// Floor 関数が切り捨てを行うことを確認します。
        /// </summary>
        [TestCase(2.7f, ExpectedResult = 2.0f)]
        public float Floor_RoundsDown(float value) {
            return FloatMath.Floor(value);
        }

        /// <summary>
        /// Ceil 関数が切り上げを行うことを確認します。
        /// </summary>
        [TestCase(2.3f, ExpectedResult = 3.0f)]
        public float Ceil_RoundsUp(float value) {
            return FloatMath.Ceil(value);
        }

        /// <summary>
        /// Round 関数が四捨五入を行うことを確認します。
        /// </summary>
        [TestCase(2.5f, ExpectedResult = 2.0f)]
        [TestCase(2.6f, ExpectedResult = 3.0f)]
        public float Round_RoundsToNearest(float value) {
            return FloatMath.Round(value);
        }

        /// <summary>
        /// RoundToInt 関数が四捨五入して整数を返すことを確認します。
        /// </summary>
        [TestCase(2.3f, ExpectedResult = 2)]
        [TestCase(2.5f, ExpectedResult = 2)]
        [TestCase(2.6f, ExpectedResult = 3)]
        [TestCase(-2.3f, ExpectedResult = -2)]
        [TestCase(-2.5f, ExpectedResult = -2)]
        [TestCase(-2.6f, ExpectedResult = -3)]
        public int RoundToInt_RoundsToNearestInt(float value) {
            return FloatMath.RoundToInt(value);
        }

        /// <summary>
        /// Sign 関数が符号を返すことを確認します。
        /// </summary>
        [TestCase(1.0f, ExpectedResult = 1)]
        [TestCase(-1.0f, ExpectedResult = -1)]
        public int Sign_ReturnsSign(float value) {
            return FloatMath.Sign(value);
        }

        /// <summary>
        /// DeltaAngle 関数が最小角度差を返すことを確認します。
        /// </summary>
        [TestCase(10.0f, 350.0f, ExpectedResult = -20.0f)]
        public float DeltaAngle_ReturnsShortestDelta(float current, float target) {
            return FloatMath.DeltaAngle(current, target);
        }

        /// <summary>
        /// Approximately 関数が誤差内の一致を正しく判定することを確認します。
        /// </summary>
        [TestCase(1.0000001f, 1.0000002f, ExpectedResult = true)]
        [TestCase(1.0f, 1.0001f, ExpectedResult = false)]
        [TestCase(0.0f, 0.0f, ExpectedResult = true)]
        public bool Approximately_ReturnsCorrectComparison(float a, float b) {
            return FloatMath.Approximately(a, b);
        }
    }
}