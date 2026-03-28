using System.Collections;
using GameFramework.VfxSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameFramework.Tests {
    /// <summary>
    /// VfxManager の基本動作テスト
    /// </summary>
    public class VfxManagerTests {
        /// <summary>
        /// モック実装：テスト用の簡易 VfxComponent
        /// </summary>
        private class MockVfxComponent : MonoBehaviour, IVfxComponent {
            void IVfxComponent.Play() => IsPlaying = true;
            void IVfxComponent.Stop() => IsPlaying = false;
            void IVfxComponent.StopImmediate() => IsPlaying = false;
            
            public bool IsPlaying { get; private set; }

            void IVfxComponent.Tick(float deltaTime) {
            }

            void IVfxComponent.SetSpeed(float speed) {
            }

            void IVfxComponent.SetLodLevel(int level) {
            }
        }

        /// <summary>
        /// モック実装：一定時間後に停止する VfxComponent
        /// </summary>
        private class TimedMockVfxComponent : MonoBehaviour, IVfxComponent {
            public bool IsPlaying { get; private set; }

            private float _elapsedTime;

            void IVfxComponent.Play() {
                _elapsedTime = 0.0f;
                IsPlaying = true;
            }

            void IVfxComponent.Stop() {
                IsPlaying = false;
            }

            void IVfxComponent.StopImmediate() {
                IsPlaying = false;
            }

            void IVfxComponent.Tick(float deltaTime) {
                if (!IsPlaying) {
                    return;
                }

                _elapsedTime += deltaTime;
                if (_elapsedTime >= 0.01f) {
                    IsPlaying = false;
                }
            }

            void IVfxComponent.SetSpeed(float speed) {
            }

            void IVfxComponent.SetLodLevel(int level) {
            }
        }

        private GameObject _prefab;
        private VfxManager _manager;
        private UpdateScheduler _updateScheduler;

        /// <summary>
        /// テスト前にモックプレハブとVfxManagerを初期化
        /// </summary>
        [SetUp]
        public void Setup() {
            _manager = new VfxManager();
            _prefab = new GameObject("VfxPrefab");
            _prefab.AddComponent<MockVfxComponent>();
            
            _updateScheduler = new UpdateScheduler();
            _updateScheduler.RegisterLateUpdatable(_manager, 0);
        }

        /// <summary>
        /// テスト後に生成物を破棄
        /// </summary>
        [TearDown]
        public void Teardown() {
            Object.DestroyImmediate(_prefab);
            _manager.Dispose();
            _updateScheduler.Dispose();
        }

        /// <summary>
        /// Play() でエフェクトが再生されていることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator Play_ShouldTriggerPlayOnComponent() {
            var context = new VfxContext {
                prefab = _prefab,
                localScale = Vector3.one
            };

            var handle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            Assert.That(handle.IsValid, Is.True, "Handle should be valid after Play.");
            Assert.That(handle.IsPlaying, Is.True, "Handle should report playing state.");
        }

        /// <summary>
        /// Stop() によりエフェクトが停止することを検証
        /// </summary>
        [UnityTest]
        public IEnumerator Stop_ShouldTriggerStopOnComponent() {
            var context = new VfxContext {
                prefab = _prefab,
                localScale = Vector3.one
            };

            var handle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            handle.Stop(immediate: true);
            ManualUpdate();
            yield return null;

            Assert.That(handle.IsPlaying, Is.False, "Handle should not report playing after Stop.");
        }

        /// <summary>
        /// Dispose() により Handle が無効になることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator Dispose_Handle_ShouldCleanupAndInvalidate() {
            var context = new VfxContext {
                prefab = _prefab,
                localScale = Vector3.one
            };

            var handle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            handle.Dispose();
            ManualUpdate();
            yield return null;

            Assert.That(handle.IsValid, Is.False, "Handle should become invalid after Dispose.");
        }

        /// <summary>
        /// 自動廃棄(autoDispose)で Handle が自動的に無効になることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator AutoDispose_ShouldReleaseAfterPlayEnds() {
            var context = new VfxContext {
                prefab = _prefab,
                localScale = Vector3.one
            };

            var handle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            handle.Stop(immediate: true, autoDispose: true);
            ManualUpdate();
            yield return null;

            Assert.That(handle.IsValid, Is.False, "Handle should auto-dispose after playback ends.");
        }

        /// <summary>
        /// 無効化済み Handle が後続の再生を操作しないことを検証
        /// </summary>
        [UnityTest]
        public IEnumerator ReleasedHandle_ShouldNotAffectNextPlayback() {
            var context = new VfxContext {
                prefab = _prefab,
                localScale = Vector3.one
            };

            var releasedHandle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            releasedHandle.Stop(immediate: true, autoDispose: true);
            ManualUpdate();
            yield return null;

            Assert.That(releasedHandle.IsValid, Is.False, "Released handle should stay invalid.");

            var nextHandle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            releasedHandle.Stop(immediate: true);
            ManualUpdate();
            yield return null;

            Assert.That(releasedHandle.IsValid, Is.False, "Released handle should not become valid again.");
            Assert.That(nextHandle.IsPlaying, Is.True, "Released handle should not stop the next playback.");
        }

        /// <summary>
        /// 自動廃棄後に古い Handle が別インスタンスへ再接続しないことを検証
        /// </summary>
        [UnityTest]
        public IEnumerator ReleasedHandle_ShouldNotControlReusedPlayingInfo() {
            Object.DestroyImmediate(_prefab);
            _prefab = new GameObject("TimedVfxPrefab");
            _prefab.AddComponent<TimedMockVfxComponent>();

            var context = new VfxContext {
                prefab = _prefab,
                localScale = Vector3.one
            };

            var oldHandle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            ManualUpdate();
            yield return null;

            Assert.That(oldHandle.IsValid, Is.False, "Old handle should become invalid after auto-dispose.");

            var newHandle = _manager.Play(context);
            ManualUpdate();
            yield return null;

            Assert.That(newHandle.IsValid, Is.True, "New handle should be valid.");
            oldHandle.Stop(immediate: true);

            Assert.That(oldHandle.IsValid, Is.False, "Old handle should remain invalid after a new play session starts.");
            Assert.That(newHandle.IsPlaying, Is.True, "Old handle should not stop the new play session.");
        }

        /// <summary>
        /// 手動更新更新
        /// </summary>
        private void ManualUpdate() {
            _updateScheduler.Update();
            _updateScheduler.LateUpdate();
            _updateScheduler.FixedUpdate();
        }
    }
}
