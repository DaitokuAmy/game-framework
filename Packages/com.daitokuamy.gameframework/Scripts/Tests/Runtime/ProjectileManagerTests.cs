using System;
using System.Reflection;
using GameFramework.ProjectileSystem;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Tests {
    /// <summary>
    /// ProjectileManager tests.
    /// </summary>
    public class ProjectileManagerTests {
        /// <summary>
        /// Test projectile implementation.
        /// </summary>
        private class MockBulletProjectile : MonoBehaviour, IBulletProjectile {
            public static int StartCount { get; set; }
            public static int TickCount { get; set; }
            public static int StopCount { get; set; }
            public static int ExitImmediateCount { get; set; }

            public IBulletProjectileController Controller { get; private set; }

            public bool IsPlaying { get; private set; }

            float IBulletProjectile.RaycastRadius => 0.0f;

            void IDisposable.Dispose() {
                Destroy(gameObject);
            }

            void IBulletProjectile.SetSpeed(float speed) {
            }

            void IBulletProjectile.SetActive(bool active) {
                gameObject.SetActive(active);
            }

            void IBulletProjectile.Play(IBulletProjectileController projectileController) {
                if (IsPlaying) {
                    return;
                }

                StartCount++;
                Controller = projectileController;
                IsPlaying = true;
            }

            void IBulletProjectile.Tick(float deltaTime) {
                TickCount++;
            }

            void IBulletProjectile.Stop() {
                StopCount++;
            }

            void IBulletProjectile.StopImmediate() {
                ExitImmediateCount++;
                Controller = null;
                IsPlaying = false;
            }

            void IBulletProjectile.SetLocalScale(Vector3 scale) {
                transform.localScale = scale;
            }

            void IBulletProjectile.OnHitCollision(RaycastHit hit) {
            }
        }

        /// <summary>
        /// Test projectile that finishes after the first update.
        /// </summary>
        private class AutoEndingBulletProjectile : MonoBehaviour, IBulletProjectile {
            public static int StartCount { get; set; }
            public static int TickCount { get; set; }
            public static int StopCount { get; set; }
            public static int StopImmediateCount { get; set; }

            public IBulletProjectileController Controller { get; private set; }

            public bool IsPlaying { get; private set; }

            float IBulletProjectile.RaycastRadius => 0.0f;

            void IDisposable.Dispose() {
                Destroy(gameObject);
            }

            void IBulletProjectile.SetSpeed(float speed) {
            }

            void IBulletProjectile.SetActive(bool active) {
                gameObject.SetActive(active);
            }

            void IBulletProjectile.Play(IBulletProjectileController projectileController) {
                StartCount++;
                Controller = projectileController;
                IsPlaying = true;
            }

            void IBulletProjectile.Tick(float deltaTime) {
                TickCount++;
                IsPlaying = false;
            }

            void IBulletProjectile.Stop() {
                StopCount++;
                IsPlaying = false;
            }

            void IBulletProjectile.StopImmediate() {
                StopImmediateCount++;
                Controller = null;
                IsPlaying = false;
            }

            void IBulletProjectile.SetLocalScale(Vector3 scale) {
                transform.localScale = scale;
            }

            void IBulletProjectile.OnHitCollision(RaycastHit hit) {
            }
        }

        /// <summary>
        /// Test controller implementation.
        /// </summary>
        private class MockBulletProjectileController : IBulletProjectileController {
            public Vector3 Position => Vector3.zero;

            public Quaternion Rotation => Quaternion.identity;

            void IProjectileController.Play() {
            }

            bool IProjectileController.Tick(float deltaTime) {
                return true;
            }

            void IProjectileController.Stop(Vector3? stopPosition) {
            }
        }

        private GameObject _prefab;
        private ProjectileManager _manager;
        private IBulletProjectileController _controller;

        /// <summary>
        /// Initialize test fixtures.
        /// </summary>
        [SetUp]
        public void Setup() {
            MockBulletProjectile.StartCount = 0;
            MockBulletProjectile.TickCount = 0;
            MockBulletProjectile.StopCount = 0;
            MockBulletProjectile.ExitImmediateCount = 0;

            AutoEndingBulletProjectile.StartCount = 0;
            AutoEndingBulletProjectile.TickCount = 0;
            AutoEndingBulletProjectile.StopCount = 0;
            AutoEndingBulletProjectile.StopImmediateCount = 0;

            _manager = new ProjectileManager();
            _controller = new MockBulletProjectileController();

            _prefab = new GameObject("ProjectilePrefab");
            _prefab.AddComponent<MockBulletProjectile>();
        }

        /// <summary>
        /// Destroy test fixtures.
        /// </summary>
        [TearDown]
        public void Teardown() {
            if (_prefab != null) {
                Object.DestroyImmediate(_prefab);
                _prefab = null;
            }

            _manager.Dispose();
        }

        /// <summary>
        /// Released handles stay invalid after reuse.
        /// </summary>
        [Test]
        public void Dispose_ShouldInvalidateReleasedHandleAndAllowReusingPooledProjectile() {
            var firstHandle = _manager.Play(_prefab, _controller, Vector3.one);
            Assert.That(firstHandle.IsValid, Is.True);

            firstHandle.Dispose();
            Assert.That(firstHandle.IsValid, Is.False);
            Assert.That(MockBulletProjectile.ExitImmediateCount, Is.EqualTo(1));

            var secondHandle = _manager.Play(_prefab, _controller, Vector3.one);
            Assert.That(MockBulletProjectile.StartCount, Is.EqualTo(2));
            Assert.That(firstHandle.IsValid, Is.False);
            Assert.That(secondHandle.IsValid, Is.True);

            firstHandle.Dispose();
            Assert.That(MockBulletProjectile.ExitImmediateCount, Is.EqualTo(1));
            Assert.That(secondHandle.IsValid, Is.True);

            secondHandle.Dispose();
            Assert.That(MockBulletProjectile.ExitImmediateCount, Is.EqualTo(2));
            Assert.That(secondHandle.IsValid, Is.False);
        }

        /// <summary>
        /// Released handles stay invalid after Clear.
        /// </summary>
        [Test]
        public void Clear_ShouldInvalidateReleasedHandleAndAllowReusingPooledProjectile() {
            var firstHandle = _manager.Play(_prefab, _controller, Vector3.one);
            Assert.That(firstHandle.IsValid, Is.True);

            _manager.Clear();
            Assert.That(firstHandle.IsValid, Is.False);
            Assert.That(MockBulletProjectile.ExitImmediateCount, Is.EqualTo(1));

            var secondHandle = _manager.Play(_prefab, _controller, Vector3.one);
            Assert.That(MockBulletProjectile.StartCount, Is.EqualTo(2));
            Assert.That(firstHandle.IsValid, Is.False);
            Assert.That(secondHandle.IsValid, Is.True);

            secondHandle.Dispose();
            Assert.That(MockBulletProjectile.ExitImmediateCount, Is.EqualTo(2));
        }

        /// <summary>
        /// Natural completion should survive follow-up updates.
        /// </summary>
        [Test]
        public void NaturalCompletion_ShouldSurviveFollowUpUpdates() {
            var prefab = CreatePrefab<AutoEndingBulletProjectile>("AutoEndingProjectilePrefab");
            try {
                var handle = _manager.Play(prefab, _controller, Vector3.one);
                Assert.That(handle.IsValid, Is.True);

                InvokeLateUpdate(_manager);
                InvokeLateUpdate(_manager);

                Assert.That(AutoEndingBulletProjectile.StartCount, Is.EqualTo(1));
                Assert.That(AutoEndingBulletProjectile.TickCount, Is.EqualTo(1));
                Assert.That(AutoEndingBulletProjectile.StopCount, Is.EqualTo(0));
                Assert.That(AutoEndingBulletProjectile.StopImmediateCount, Is.EqualTo(0));
                Assert.That(handle.IsValid, Is.False);
            }
            finally {
                Object.DestroyImmediate(prefab);
            }
        }

        /// <summary>
        /// StopAll(clear: false) should survive follow-up updates.
        /// </summary>
        [Test]
        public void StopAllWithoutClear_ShouldSurviveFollowUpUpdates() {
            var prefab = CreatePrefab<AutoEndingBulletProjectile>("StopAllProjectilePrefab");
            try {
                var handle = _manager.Play(prefab, _controller, Vector3.one);
                Assert.That(handle.IsValid, Is.True);

                _manager.StopAll();
                Assert.That(AutoEndingBulletProjectile.StopCount, Is.EqualTo(1));

                InvokeLateUpdate(_manager);
                InvokeLateUpdate(_manager);

                Assert.That(AutoEndingBulletProjectile.StartCount, Is.EqualTo(1));
                Assert.That(AutoEndingBulletProjectile.TickCount, Is.EqualTo(1));
                Assert.That(AutoEndingBulletProjectile.StopImmediateCount, Is.EqualTo(0));
                Assert.That(handle.IsValid, Is.False);
            }
            finally {
                Object.DestroyImmediate(prefab);
            }
        }

        /// <summary>
        /// Create a test prefab.
        /// </summary>
        private static GameObject CreatePrefab<TProjectile>(string name)
            where TProjectile : Component {
            var prefab = new GameObject(name);
            prefab.AddComponent<TProjectile>();
            return prefab;
        }

        /// <summary>
        /// Invoke ProjectileManager's late update logic explicitly.
        /// </summary>
        private static void InvokeLateUpdate(ProjectileManager manager) {
            var method = typeof(ProjectileManager).GetMethod(
                "LateUpdateInternal",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            method.Invoke(manager, Array.Empty<object>());
        }
    }
}
