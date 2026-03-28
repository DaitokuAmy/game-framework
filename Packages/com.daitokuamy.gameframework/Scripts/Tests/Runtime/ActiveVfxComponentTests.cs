using System.Reflection;
using GameFramework.VfxSystem;
using NUnit.Framework;
using UnityEngine;

namespace GameFramework.Tests {
    /// <summary>
    /// ActiveVfxComponent の動作テスト
    /// </summary>
    public class ActiveVfxComponentTests {
        private GameObject _owner;
        private GameObject _target;
        private IVfxComponent _component;

        /// <summary>
        /// テスト前に対象を初期化
        /// </summary>
        [SetUp]
        public void Setup() {
            _owner = new GameObject("ActiveVfxOwner");
            _target = new GameObject("ActiveVfxTarget");

            var component = _owner.AddComponent<ActiveVfxComponent>();
            SetPrivateField(component, "_targetObjects", new[] { _target });
            SetPrivateField(component, "_duration", 0.1f);
            SetPrivateField(component, "_loop", true);

            _component = component;
        }

        /// <summary>
        /// テスト後に生成物を破棄
        /// </summary>
        [TearDown]
        public void Teardown() {
            Object.DestroyImmediate(_owner);
            Object.DestroyImmediate(_target);
        }

        /// <summary>
        /// loop 指定時は停止まで再生中を維持することを検証
        /// </summary>
        [Test]
        public void LoopPlayback_ShouldRemainPlayingUntilStopped() {
            _component.Play();
            _component.Tick(1.0f);

            Assert.That(_component.IsPlaying, Is.True, "Loop playback should remain playing after duration.");
            Assert.That(_target.activeSelf, Is.True, "Target object should stay active while looping.");

            _component.StopImmediate();

            Assert.That(_component.IsPlaying, Is.False, "Loop playback should stop when explicitly stopped.");
            Assert.That(_target.activeSelf, Is.False, "Target object should be inactive after StopImmediate.");
        }

        /// <summary>
        /// private フィールドを設定
        /// </summary>
        private static void SetPrivateField<T>(Object target, string fieldName, T value) {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"{fieldName} should exist.");
            field.SetValue(target, value);
        }
    }
}
