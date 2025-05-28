using System;
using GameFramework.Core;
using NUnit.Framework;

namespace GameFramework.Tests {
    /// <summary>
    /// MockのDeltaTimeProvider（60FPS相当）
    /// </summary>
    public class MockDeltaTimeProvider : IDeltaTimeProvider {
        public float DeltaTime { get; set; } = 0.016f;
    }

    /// <summary>
    /// LayeredTimeのユニットテスト
    /// </summary>
    public class LayeredTimeTests {
        private MockDeltaTimeProvider _mockDeltaTime;

        [SetUp]
        public void SetUp() {
            _mockDeltaTime = new MockDeltaTimeProvider();
        }

        /// <summary>
        /// 親階層のないLayeredTimeが正常に初期化されること
        /// </summary>
        [Test]
        public void Initializes_WithNoParent() {
            var time = new LayeredTime(null, _mockDeltaTime);
            Assert.AreEqual(1.0f, time.TimeScale);
            Assert.AreEqual(_mockDeltaTime.DeltaTime, time.DeltaTime);
        }

        /// <summary>
        /// LocalTimeScaleを変更するとTimeScaleに正しく反映されること
        /// </summary>
        [Test]
        public void LocalTimeScale_ChangesEffectiveTimeScale() {
            var time = new LayeredTime(null, _mockDeltaTime);
            time.LocalTimeScale = 0.5f;
            Assert.AreEqual(0.5f, time.TimeScale);
            Assert.AreEqual(_mockDeltaTime.DeltaTime * 0.5f, time.DeltaTime);
        }

        /// <summary>
        /// 親のTimeScaleが子に伝播すること
        /// </summary>
        [Test]
        public void ParentTimeScale_PropagatesToChild() {
            var parent = new LayeredTime(null, _mockDeltaTime);
            var child = new LayeredTime(parent, _mockDeltaTime);

            parent.LocalTimeScale = 0.5f;
            child.LocalTimeScale = 0.25f;

            Assert.AreEqual(0.5f, parent.TimeScale);
            Assert.AreEqual(0.125f, child.TimeScale); // 0.5 * 0.25
        }

        /// <summary>
        /// TimeScale変更時にイベントが発火すること
        /// </summary>
        [Test]
        public void ChangedTimeScaleEvent_IsInvoked() {
            var time = new LayeredTime(null, _mockDeltaTime);
            float lastScale = -1;

            time.ChangedTimeScaleEvent += scale => lastScale = scale;
            time.LocalTimeScale = 0.5f;

            Assert.AreEqual(0.5f, lastScale);
        }

        /// <summary>
        /// 循環参照を設定しようとすると例外がスローされること
        /// </summary>
        [Test]
        public void SetParent_ThrowsOnCircularReference() {
            var root = new LayeredTime(null, _mockDeltaTime);
            var mid = new LayeredTime(root, _mockDeltaTime);
            var leaf = new LayeredTime(mid, _mockDeltaTime);

            Assert.Throws<InvalidOperationException>(() => root.SetParent(leaf));
        }

        /// <summary>
        /// Disposeで親子関係が正しく解消されること
        /// </summary>
        [Test]
        public void Dispose_ClearsRelationships() {
            var parent = new LayeredTime(null, _mockDeltaTime);
            var child = new LayeredTime(parent, _mockDeltaTime);

            child.Dispose();

            Assert.IsNull(child.Parent);
            Assert.AreEqual(0, GetChildrenCount(parent));
        }

        /// <summary>
        /// 内部の_childrenリストの要素数を取得するヘルパー（リフレクション使用）
        /// </summary>
        private int GetChildrenCount(LayeredTime time) {
            var field = typeof(LayeredTime).GetField("_children", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = field?.GetValue(time) as System.Collections.ICollection;
            return list?.Count ?? -1;
        }
    }
}
