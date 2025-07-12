using NUnit.Framework;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Tests {
    /// <summary>
    /// FastRandom と RichRandom の動作検証テスト
    /// </summary>
    public class RandomTests {
        private const int Seed = 12345;

        /// <summary>
        /// FastRandom が同じシードで同じ乱数列を生成するかを検証
        /// </summary>
        [Test]
        public void FastRandom_ReproducibilityTest() {
            var r1 = new FastRandom(Seed);
            var r2 = new FastRandom(Seed);

            for (var i = 0; i < 100; i++) {
                Assert.AreEqual(r1.Next(), r2.Next(), $"Mismatch at index {i}");
            }
        }

        /// <summary>
        /// RichRandom が同じシードで同じ乱数列を生成するかを検証
        /// </summary>
        [Test]
        public void RichRandom_ReproducibilityTest() {
            var r1 = new RichRandom(Seed);
            var r2 = new RichRandom(Seed);

            for (var i = 0; i < 100; i++) {
                Assert.AreEqual(r1.Next(), r2.Next(), $"Mismatch at index {i}");
            }
        }

        /// <summary>
        /// FastRandom の Range(min, max) が整数範囲内に収まっているか検証
        /// </summary>
        [Test]
        public void FastRandom_IntRangeWithinBounds() {
            var rand = new FastRandom(Seed);
            for (var i = 0; i < 1000; i++) {
                var val = rand.Range(10, 20);
                Assert.IsTrue(val >= 10 && val < 20, $"Out of bounds: {val}");
            }
        }

        /// <summary>
        /// RichRandom の Range(min, max) が整数範囲内に収まっているか検証
        /// </summary>
        [Test]
        public void RichRandom_IntRangeWithinBounds() {
            var rand = new RichRandom(Seed);
            for (var i = 0; i < 1000; i++) {
                var val = rand.Range(100, 200);
                Assert.IsTrue(val >= 100 && val < 200, $"Out of bounds: {val}");
            }
        }

        /// <summary>
        /// FastRandom の Range(min, max) が float 範囲内に収まっているか検証
        /// </summary>
        [Test]
        public void FastRandom_FloatRangeWithinBounds() {
            var rand = new FastRandom(Seed);
            for (var i = 0; i < 1000; i++) {
                var val = rand.Range(0.0f, 1.0f);
                Assert.IsTrue(val >= 0.0f && val <= 1.0f, $"Out of bounds: {val}");
            }
        }

        /// <summary>
        /// RichRandom の Range(min, max) が float 範囲内に収まっているか検証
        /// </summary>
        [Test]
        public void RichRandom_FloatRangeWithinBounds() {
            var rand = new RichRandom(Seed);
            for (var i = 0; i < 1000; i++) {
                var val = rand.Range(-5.0f, 5.0f);
                Assert.IsTrue(val >= -5.0f && val <= 5.0f, $"Out of bounds: {val}");
            }
        }

        /// <summary>
        /// FastRandom が異なるシードで異なる乱数列を生成するかを検証
        /// </summary>
        [Test]
        public void FastRandom_DifferentSeedsProduceDifferentSequences() {
            var r1 = new FastRandom(Seed);
            var r2 = new FastRandom(Seed + 1);

            var different = false;
            for (var i = 0; i < 100; i++) {
                if (r1.Next() != r2.Next()) {
                    different = true;
                    break;
                }
            }

            Assert.IsTrue(different, "Sequences should differ with different seeds");
        }

        /// <summary>
        /// RichRandom が異なるシードで異なる乱数列を生成するかを検証
        /// </summary>
        [Test]
        public void RichRandom_DifferentSeedsProduceDifferentSequences() {
            var r1 = new RichRandom(Seed);
            var r2 = new RichRandom(Seed + 1);

            var different = false;
            for (var i = 0; i < 100; i++) {
                if (r1.Next() != r2.Next()) {
                    different = true;
                    break;
                }
            }

            Assert.IsTrue(different, "Sequences should differ with different seeds");
        }
    }
}
