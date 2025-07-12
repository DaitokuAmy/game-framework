using System.Collections;
using GameFramework.VfxSystems;
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

            void IVfxComponent.Update(float deltaTime) {
            }

            void IVfxComponent.SetSpeed(float speed) {
            }

            void IVfxComponent.SetLodLevel(int level) {
            }
        }

        private GameObject _prefab;
        private VfxManager _manager;
        private TaskRunner _taskRunner;

        /// <summary>
        /// テスト前にモックプレハブとVfxManagerを初期化
        /// </summary>
        [SetUp]
        public void Setup() {
            _manager = new VfxManager();
            _prefab = new GameObject("VfxPrefab");
            _prefab.AddComponent<MockVfxComponent>();
            
            _taskRunner = new TaskRunner();
            _taskRunner.Register(_manager);
        }

        /// <summary>
        /// テスト後に生成物を破棄
        /// </summary>
        [TearDown]
        public void Teardown() {
            Object.DestroyImmediate(_prefab);
            _manager.Dispose();
            _taskRunner.Dispose();
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
            TaskUpdate();
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
            TaskUpdate();
            yield return null;

            handle.Stop(immediate: true);
            TaskUpdate();
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
            TaskUpdate();
            yield return null;

            handle.Dispose();
            TaskUpdate();
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
            TaskUpdate();
            yield return null;

            handle.Stop(immediate: true, autoDispose: true);
            TaskUpdate();
            yield return null;

            Assert.That(handle.IsValid, Is.False, "Handle should auto-dispose after playback ends.");
        }

        /// <summary>
        /// タスクランナーの更新
        /// </summary>
        private void TaskUpdate() {
            _taskRunner.Update();
            _taskRunner.LateUpdate();
            _taskRunner.FixedUpdate();
        }
    }
}