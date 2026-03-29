using System;
using System.Collections.Generic;
using System.Threading;
using GameFramework.AssetSystem;
using NUnit.Framework;
#if USE_UNI_TASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.Tests {
    /// <summary>
    /// AssetSystem の回帰テスト
    /// </summary>
    public sealed class AssetSystemTests {
        /// <summary>
        /// AssetHandle のコピー解放が全コピーへ反映されることを検証
        /// </summary>
        [Test]
        public void AssetHandle_ReleaseOnCopy_InvalidatesAllCopies() {
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var info = new FakeAssetInfo<TestAsset>(asset);
                var handle = new AssetHandle<TestAsset>(info);
                var copiedHandle = handle;

                handle.Release();

                Assert.That(handle.IsValid, Is.False);
                Assert.That(copiedHandle.IsValid, Is.False);
                copiedHandle.Release();
                Assert.That(info.DisposeCount, Is.EqualTo(1));
            }
            finally {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// SceneAssetHandle のコピー解放が全コピーへ反映されることを検証
        /// </summary>
        [Test]
        public void SceneAssetHandle_ReleaseOnCopy_InvalidatesAllCopies() {
            var info = new FakeSceneAssetInfo();
            var handle = new SceneAssetHandle(info);
            var copiedHandle = handle;

            copiedHandle.Release();

            Assert.That(handle.IsValid, Is.False);
            Assert.That(copiedHandle.IsValid, Is.False);
            Assert.That(info.DisposeCount, Is.EqualTo(1));
            Assert.That(handle.ActivateAsync().Exception, Is.TypeOf<OperationCanceledException>());
        }

        /// <summary>
        /// Scope 経由解放が返却済みハンドルも無効化することを検証
        /// </summary>
        [Test]
        public void AssetRequest_LoadAsync_WithScopeExpiration_InvalidatesReturnedHandle() {
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var provider = new FakeAssetProvider();
                var info = new FakeAssetInfo<TestAsset>(asset);
                provider.RegisterAsset("asset", info);

                var assetManager = new AssetManager();
                assetManager.Initialize(provider);

                var scope = new FakeScope();
                var request = new TestAssetRequest("asset", FakeAssetProvider.KeyName);

                var handle = request.LoadAsync(assetManager, scope);
                scope.Expire();

                Assert.That(handle.IsValid, Is.False);
                Assert.That(info.DisposeCount, Is.EqualTo(1));
            }
            finally {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// 無効な Scope 指定時は即座に解放されることを検証
        /// </summary>
        [Test]
        public void AssetRequest_LoadAsync_WithInvalidScope_ReleasesHandleImmediately() {
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var provider = new FakeAssetProvider();
                var info = new FakeAssetInfo<TestAsset>(asset);
                provider.RegisterAsset("asset", info);

                var assetManager = new AssetManager();
                assetManager.Initialize(provider);

                var scope = new FakeScope();
                scope.Expire();

                var request = new TestAssetRequest("asset", FakeAssetProvider.KeyName);
                var handle = request.LoadAsync(assetManager, scope);

                Assert.That(handle.IsValid, Is.False);
                Assert.That(info.DisposeCount, Is.EqualTo(1));
            }
            finally {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// Storage が返すハンドルごとに独立した lease を持つことを検証
        /// </summary>
        [Test]
        public void SimpleAssetStorage_ReturnedHandleRelease_DoesNotInvalidateCacheLease() {
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var provider = new FakeAssetProvider();
                var info = new FakeAssetInfo<TestAsset>(asset);
                provider.RegisterAsset("asset", info);

                var assetManager = new AssetManager();
                assetManager.Initialize(provider);
                using var storage = new SimpleAssetStorage<TestAsset>(assetManager);

                var request = new TestAssetRequest("asset", FakeAssetProvider.KeyName);
                var firstHandle = storage.LoadAssetAsync(request);
                var secondHandle = storage.LoadAssetAsync(request);

                firstHandle.Release();

                Assert.That(secondHandle.IsValid, Is.True);
                Assert.That(storage.GetAsset(request), Is.SameAs(asset));
                Assert.That(info.DisposeCount, Is.EqualTo(0));

                secondHandle.Release();

                Assert.That(info.DisposeCount, Is.EqualTo(0));
            }
            finally {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// 直接値を返す拡張APIでは Scope が必須であることを検証
        /// </summary>
#if USE_UNI_TASK
        [Test]
        public void AssetRequestExtensions_LoadAsync_WithoutScope_ThrowsInvalidOperationException() {
            var assetManager = new AssetManager();
            assetManager.Initialize(new FakeAssetProvider());
            var request = new TestAssetRequest("asset", FakeAssetProvider.KeyName);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await request.LoadAsync(assetManager, unloadScope: null).AsTask());
        }
#endif

        /// <summary>
        /// SimpleSceneAssetStorage が mode ごとに別キャッシュを持つことを検証
        /// </summary>
        [Test]
        public void SimpleSceneAssetStorage_DifferentModes_UseDifferentCacheEntries() {
            var provider = new FakeAssetProvider();
            var assetManager = new AssetManager();
            assetManager.Initialize(provider);
            using var storage = new SimpleSceneAssetStorage(assetManager);

            var singleRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Single, FakeAssetProvider.KeyName);
            var additiveRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Additive, FakeAssetProvider.KeyName);

            _ = storage.LoadAssetAsync(singleRequest);
            _ = storage.LoadAssetAsync(additiveRequest);
            _ = storage.LoadAssetAsync(singleRequest);

            Assert.That(provider.SceneLoadRequests.Count, Is.EqualTo(2));
            Assert.That(provider.SceneLoadRequests[0], Is.EqualTo(("scene", LoadSceneMode.Single)));
            Assert.That(provider.SceneLoadRequests[1], Is.EqualTo(("scene", LoadSceneMode.Additive)));
        }

        /// <summary>
        /// PoolSceneAssetStorage が mode ごとに別キャッシュを持つことを検証
        /// </summary>
        [Test]
        public void PoolSceneAssetStorage_DifferentModes_UseDifferentCacheEntries() {
            var provider = new FakeAssetProvider();
            var assetManager = new AssetManager();
            assetManager.Initialize(provider);
            using var storage = new PoolSceneAssetStorage(assetManager, amount: 4);

            var singleRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Single, FakeAssetProvider.KeyName);
            var additiveRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Additive, FakeAssetProvider.KeyName);

            _ = storage.LoadAssetAsync(singleRequest);
            _ = storage.LoadAssetAsync(additiveRequest);
            _ = storage.LoadAssetAsync(singleRequest);

            Assert.That(provider.SceneLoadRequests.Count, Is.EqualTo(2));
            Assert.That(provider.SceneLoadRequests[0], Is.EqualTo(("scene", LoadSceneMode.Single)));
            Assert.That(provider.SceneLoadRequests[1], Is.EqualTo(("scene", LoadSceneMode.Additive)));
        }

        /// <summary>
        /// PreloadSceneAssetStorage が mode ごとに別キャッシュを持つことを検証
        /// </summary>
        [Test]
        public void PreloadSceneAssetStorage_DifferentModes_UseDifferentCacheEntries() {
            var provider = new FakeAssetProvider();
            var assetManager = new AssetManager();
            assetManager.Initialize(provider);
            using var storage = new PreloadSceneAssetStorage(assetManager);

            var singleRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Single, FakeAssetProvider.KeyName);
            var additiveRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Additive, FakeAssetProvider.KeyName);

            _ = storage.LoadAssetsAsync(new[] { singleRequest, additiveRequest });
            _ = storage.LoadAssetAsync(singleRequest);

            Assert.That(provider.SceneLoadRequests.Count, Is.EqualTo(2));
            Assert.That(provider.SceneLoadRequests[0], Is.EqualTo(("scene", LoadSceneMode.Single)));
            Assert.That(provider.SceneLoadRequests[1], Is.EqualTo(("scene", LoadSceneMode.Additive)));
        }

        /// <summary>
        /// SceneStorage の mode 指定アンロードが該当エントリだけを解放することを検証
        /// </summary>
        [Test]
        public void SimpleSceneAssetStorage_UnloadAssetByRequest_ReleasesOnlyMatchedMode() {
            var provider = new FakeAssetProvider();
            var assetManager = new AssetManager();
            assetManager.Initialize(provider);
            using var storage = new SimpleSceneAssetStorage(assetManager);

            var singleRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Single, FakeAssetProvider.KeyName);
            var additiveRequest = new TestSceneAssetRequest("scene", LoadSceneMode.Additive, FakeAssetProvider.KeyName);

            var singleHandle = storage.LoadAssetAsync(singleRequest);
            var additiveHandle = storage.LoadAssetAsync(additiveRequest);

            storage.UnloadAsset(singleRequest);

            Assert.That(singleHandle.IsValid, Is.False);
            Assert.That(additiveHandle.IsValid, Is.True);
            Assert.That(storage.GetAsset(additiveRequest), Is.EqualTo(additiveHandle.Scene));
        }

        /// <summary>
        /// テスト用アセット
        /// </summary>
        private sealed class TestAsset : ScriptableObject {
        }

        /// <summary>
        /// テスト用 AssetInfo
        /// </summary>
        private sealed class FakeAssetInfo<TAsset> : IAssetInfo<TAsset>
            where TAsset : UnityEngine.Object {
            public bool IsDone => true;
            public TAsset Asset { get; }
            public Exception Exception => null;
            public int DisposeCount { get; private set; }

            public FakeAssetInfo(TAsset asset) {
                Asset = asset;
            }

            public void Dispose() {
                DisposeCount++;
            }
        }

        /// <summary>
        /// テスト用 SceneInfo
        /// </summary>
        private sealed class FakeSceneAssetInfo : ISceneAssetInfo {
            public bool IsDone => true;
            public Scene Scene => new Scene();
            public Exception Exception => null;
            public int DisposeCount { get; private set; }

            public void Dispose() {
                DisposeCount++;
            }

            public AsyncOperation ActivateAsync() {
                return null;
            }
        }

        /// <summary>
        /// テスト用 Scope
        /// </summary>
        private sealed class FakeScope : IScope {
            public event Action ExpiredEvent;
            public bool IsValid { get; private set; } = true;
            public CancellationToken Token => CancellationToken.None;

            public void Expire() {
                IsValid = false;
                ExpiredEvent?.Invoke();
            }
        }

        /// <summary>
        /// テスト用 Provider
        /// </summary>
        private sealed class FakeAssetProvider : IAssetProvider {
            public const string KeyName = "Fake";
            private readonly Dictionary<string, IAssetInfo<TestAsset>> _assetInfos = new();

            public string Key => KeyName;
            public List<(string Address, LoadSceneMode Mode)> SceneLoadRequests { get; } = new();

            public void RegisterAsset(string address, IAssetInfo<TestAsset> info) {
                _assetInfos[address] = info;
            }

            public AssetHandle<T> LoadAsync<T>(string address)
                where T : UnityEngine.Object {
                if (typeof(T) == typeof(TestAsset) && _assetInfos.TryGetValue(address, out var info)) {
                    return new AssetHandle<T>((IAssetInfo<T>)info);
                }

                return AssetHandle<T>.Empty;
            }

            public SceneAssetHandle LoadSceneAsync(string address, LoadSceneMode mode) {
                SceneLoadRequests.Add((address, mode));
                return new SceneAssetHandle(new FakeSceneAssetInfo());
            }
        }

        /// <summary>
        /// テスト用 AssetRequest
        /// </summary>
        private sealed class TestAssetRequest : AssetRequest<TestAsset> {
            private readonly string _address;
            private readonly string[] _providerKeys;

            public override string Address => _address;
            public override string[] ProviderKeys => _providerKeys;

            public TestAssetRequest(string address, params string[] providerKeys) {
                _address = address;
                _providerKeys = providerKeys;
            }
        }

        /// <summary>
        /// テスト用 SceneAssetRequest
        /// </summary>
        private sealed class TestSceneAssetRequest : SceneAssetRequest {
            private readonly string _address;
            private readonly LoadSceneMode _mode;
            private readonly string[] _providerKeys;

            public override LoadSceneMode Mode => _mode;
            public override string Address => _address;
            public override string[] ProviderKeys => _providerKeys;

            public TestSceneAssetRequest(string address, LoadSceneMode mode, params string[] providerKeys) {
                _address = address;
                _mode = mode;
                _providerKeys = providerKeys;
            }
        }
    }
}
