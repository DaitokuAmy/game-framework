using System;
using System.Reflection;
using GameFramework.CutsceneSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GameFramework.Tests {
    /// <summary>
    /// CutsceneManager の回帰テスト
    /// </summary>
    public sealed class CutsceneManagerTests {
        /// <summary>
        /// テスト用の簡易カットシーン
        /// </summary>
        private sealed class MockCutsceneComponent : MonoBehaviour, ICutscene {
            public bool IsPlaying { get; private set; }

            public float LastSeekTime { get; private set; }

            public float LastPlayTime { get; private set; }

            void IDisposable.Dispose() {
                IsPlaying = false;
            }

            void ICutscene.Initialize(bool updateGameTime) {
            }

            void ICutscene.OnReturn() {
                IsPlaying = false;
                gameObject.SetActive(false);
            }

            void ICutscene.Play() {
                LastPlayTime = LastSeekTime;
                IsPlaying = true;
            }

            void ICutscene.Stop() {
                IsPlaying = false;
                gameObject.SetActive(false);
            }

            void ICutscene.Update(float deltaTime) {
            }

            void ICutscene.SetSpeed(float speed) {
            }

            void ICutscene.Seek(float time) {
                LastSeekTime = time;
            }
        }

        /// <summary>
        /// シーク通知確認用の Cutscene 実装
        /// </summary>
        private sealed class TrackingCutsceneComponent : Cutscene {
            public float LastSeekTime { get; private set; }

            protected override void SeekInternal(float time) {
                LastSeekTime = time;
            }
        }

        private GameObject _prefab;
        private CutsceneManager _manager;

        /// <summary>
        /// テスト前の初期化
        /// </summary>
        [SetUp]
        public void Setup() {
            _manager = new CutsceneManager();
            _prefab = new GameObject("CutscenePrefab");
            _prefab.AddComponent<MockCutsceneComponent>();
        }

        /// <summary>
        /// テスト後の後始末
        /// </summary>
        [TearDown]
        public void TearDown() {
            if (_prefab != null) {
                Object.DestroyImmediate(_prefab);
                _prefab = null;
            }

            _manager.Dispose();
        }

        /// <summary>
        /// 解放済み Handle が再利用済み PlayingInfo を操作しないこと
        /// </summary>
        [Test]
        public void ReleasedHandle_ShouldNotControlReusedPlayingInfo() {
            var firstHandle = _manager.GetHandle(_prefab);
            firstHandle.Play();
            Assert.That(firstHandle.IsPlaying, Is.True);

            firstHandle.Dispose();
            Assert.That(GetPlayingInfoCount(_manager), Is.EqualTo(0));

            var secondHandle = _manager.GetHandle(_prefab);
            secondHandle.Play();
            Assert.That(secondHandle.IsPlaying, Is.True);

            firstHandle.Stop();

            Assert.That(firstHandle.IsPlaying, Is.False);
            Assert.That(secondHandle.IsPlaying, Is.True);
        }

        /// <summary>
        /// Clear 後に再生中リストが残らないこと
        /// </summary>
        [Test]
        public void Clear_ShouldRemoveAllPlayingInfos() {
            var handle = _manager.Play(_prefab);
            Assert.That(handle.IsPlaying, Is.True);

            _manager.Clear();

            Assert.That(GetPlayingInfoCount(_manager), Is.EqualTo(0));
            Assert.DoesNotThrow(() => InvokeLateUpdate(_manager));

            var nextHandle = _manager.Play(_prefab);
            Assert.That(nextHandle.IsPlaying, Is.True);
        }

        /// <summary>
        /// Dispose が即時に Pool 返却されること
        /// </summary>
        [Test]
        public void Dispose_ShouldReleasePlayingInfoImmediately() {
            var handle = _manager.GetHandle(_prefab);
            handle.Play();

            Assert.That(GetPlayingInfoCount(_manager), Is.EqualTo(1));

            handle.Dispose();

            Assert.That(GetPlayingInfoCount(_manager), Is.EqualTo(0));

            var nextHandle = _manager.GetHandle(_prefab);
            nextHandle.Play();

            Assert.That(nextHandle.IsPlaying, Is.True);
        }

        /// <summary>
        /// SetTime 後の Play が指定位置から始まること
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator SetTimeBeforePlay_ShouldStartFromRequestedTime() {
            var scene = SceneManager.CreateScene("CutsceneManagerTests.StartTimeScene");
            var root = new GameObject("StartTimeCutscene");
            var cutscene = root.AddComponent<MockCutsceneComponent>();
            SceneManager.MoveGameObjectToScene(root, scene);

            var handle = _manager.GetHandle<MockCutsceneComponent>(scene, _ => { });

            handle.SetTime(1.25f);
            handle.Play();

            Assert.That(cutscene.LastPlayTime, Is.EqualTo(1.25f));

            handle.Stop();
            handle.Play();

            Assert.That(cutscene.LastPlayTime, Is.EqualTo(0.0f));

            handle.Dispose();
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>
        /// Cutscene 継承なしの Scene 実装でも再生できること
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator SceneHandle_ShouldSupportNonCutsceneMonoBehaviour() {
            var scene = SceneManager.CreateScene("CutsceneManagerTests.MockScene");
            var root = new GameObject("SceneCutscene");
            root.AddComponent<MockCutsceneComponent>();
            SceneManager.MoveGameObjectToScene(root, scene);

            var handle = _manager.GetHandle<MockCutsceneComponent>(scene, _ => { });

            Assert.That(handle.IsDone, Is.True);

            handle.Play();

            Assert.That(handle.IsPlaying, Is.True);

            handle.Dispose();
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>
        /// SetTime が SeekInternal を呼び出すこと
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator SetTime_ShouldInvokeSeekInternal() {
            var scene = SceneManager.CreateScene("CutsceneManagerTests.TrackingScene");
            var root = new GameObject("TrackingCutscene");
            root.AddComponent<PlayableDirector>();
            var cutscene = root.AddComponent<TrackingCutsceneComponent>();
            SceneManager.MoveGameObjectToScene(root, scene);

            var handle = _manager.GetHandle<TrackingCutsceneComponent>(scene, _ => { });
            handle.SetTime(1.25f);

            Assert.That(cutscene.LastSeekTime, Is.EqualTo(1.25f));

            handle.Dispose();
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>
        /// LateUpdateInternal を直接呼び出す
        /// </summary>
        private static void InvokeLateUpdate(CutsceneManager manager) {
            var method = typeof(CutsceneManager).GetMethod(
                "LateUpdateInternal",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            method.Invoke(manager, Array.Empty<object>());
        }

        /// <summary>
        /// 再生中情報の数を取得する
        /// </summary>
        private static int GetPlayingInfoCount(CutsceneManager manager) {
            var field = typeof(CutsceneManager).GetField(
                "_playingInfos",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);
            var list = field.GetValue(manager) as System.Collections.ICollection;
            Assert.That(list, Is.Not.Null);
            return list.Count;
        }
    }
}
