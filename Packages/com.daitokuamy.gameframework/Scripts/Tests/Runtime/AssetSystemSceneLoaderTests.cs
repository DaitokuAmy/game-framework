using System.Collections;
using GameFramework.AssetSystem;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GameFramework.Tests {
    /// <summary>
    /// AssetSystem のシーンローダー回帰テスト
    /// </summary>
    public sealed class AssetSystemSceneLoaderTests {
        private const string TestScenePath = "Packages/com.daitokuamy.gameframework/TestAssets/Scenes/asset_system_test_scene.unity";

#if UNITY_EDITOR
        /// <summary>
        /// AssetDatabaseSceneLoader がアクティブ化前に完了扱いになることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator AssetDatabaseSceneLoader_LoadAsync_WithManualActivation_CompletesBeforeActivate() {
            yield return EnsureSceneUnloaded(TestScenePath);

            var loader = new AssetDatabaseSceneLoader();
            var request = new SceneRequest(TestScenePath, activateOnLoad: false);

            Assert.That(loader.CanLoad(request), Is.True);

            using var handle = loader.LoadAsync(request);

            var isCompletedBeforeActivate = false;
            for (var i = 0; i < 120; i++) {
                if (handle.IsDone) {
                    isCompletedBeforeActivate = true;
                    break;
                }

                yield return null;
            }

            Assert.That(isCompletedBeforeActivate, Is.True);
            Assert.That(handle.Exception, Is.Null);

            yield return handle.ActivateAsync();

            var scene = SceneManager.GetSceneByPath(TestScenePath);
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(scene.isLoaded, Is.True);

            handle.Release();
            yield return EnsureSceneUnloaded(TestScenePath);
        }

        /// <summary>
        /// シーンが読み込まれていればアンロード
        /// </summary>
        private static IEnumerator EnsureSceneUnloaded(string scenePath) {
            var scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid() || !scene.isLoaded) {
                yield break;
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }
#endif
    }
}
