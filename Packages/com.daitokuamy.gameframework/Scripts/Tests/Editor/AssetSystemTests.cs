using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.AssetSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GameFramework.Tests {
    /// <summary>
    /// AssetSystem の回帰テスト
    /// </summary>
    public sealed class AssetSystemTests {
        /// <summary>
        /// 同じ要求は同じキャッシュを再利用することを検証
        /// </summary>
        [Test]
        public void SimpleAssetStorage_LoadAsync_SameRequest_UsesSingleCacheEntry() {
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var loader = new FakeAssetLoader();
                loader.Register("asset", asset);

                using var storage = new SimpleAssetStorage(loader);
                var first = storage.LoadAsync<TestAsset>("asset");
                var second = storage.LoadAsync<TestAsset>("asset");

                Assert.That(loader.LoadCount, Is.EqualTo(1));
                Assert.That(first.Result, Is.SameAs(asset));
                Assert.That(second.Result, Is.SameAs(asset));
            }
            finally {
                Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// カスタムRequest経由でも読み込みできることを検証
        /// </summary>
        [Test]
        public void SimpleAssetStorage_LoadAsync_WithCustomRequest_LoadsAsset() {
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var loader = new FakeAssetLoader();
                loader.Register("custom", asset);

                using var storage = new SimpleAssetStorage(loader);
                var process = storage.LoadAsync<TestAsset, TestAssetRequest>(new TestAssetRequest("custom"));

                Assert.That(process.Result, Is.SameAs(asset));
            }
            finally {
                Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// Unloadで保持中ハンドルが解放されることを検証
        /// </summary>
        [Test]
        public void SimpleAssetStorage_Unload_ReleasesCachedHandle() {
            var asset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var loader = new FakeAssetLoader();
                loader.Register("asset", asset);

                using var storage = new SimpleAssetStorage(loader);
                _ = storage.LoadAsync<TestAsset>("asset");

                storage.Unload<TestAsset>("asset");

                Assert.That(loader.GetReleaseCount("asset"), Is.EqualTo(1));
            }
            finally {
                Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// LRU上限超過時に最古のキャッシュを解放することを検証
        /// </summary>
        [Test]
        public void LruAssetStorage_LoadAsync_WhenCapacityExceeded_ReleasesLeastRecentlyUsed() {
            var firstAsset = ScriptableObject.CreateInstance<TestAsset>();
            var secondAsset = ScriptableObject.CreateInstance<TestAsset>();
            try {
                var loader = new FakeAssetLoader();
                loader.Register("first", firstAsset);
                loader.Register("second", secondAsset);

                using var storage = new LruAssetStorage(1, loader);
                _ = storage.LoadAsync<TestAsset>("first");
                _ = storage.LoadAsync<TestAsset>("second");

                Assert.That(loader.GetReleaseCount("first"), Is.EqualTo(1));
                Assert.That(loader.GetReleaseCount("second"), Is.EqualTo(0));
            }
            finally {
                Object.DestroyImmediate(firstAsset);
                Object.DestroyImmediate(secondAsset);
            }
        }

        /// <summary>
        /// キャッシュヒット時にActivateOnLoadが反映されることを検証
        /// </summary>
        [Test]
        public void SimpleSceneStorage_LoadAsync_CacheHitWithActivateOnLoad_ActivatesScene() {
            var loader = new FakeSceneLoader();
            loader.Register("scene");

            using var storage = new SimpleSceneStorage(loader);
            _ = storage.LoadAsync(new SceneRequest("scene", activateOnLoad: false));
            var process = storage.LoadAsync(new SceneRequest("scene", activateOnLoad: true));

            Assert.That(loader.LoadCount, Is.EqualTo(1));
            Assert.That(loader.GetActivateCount("scene"), Is.EqualTo(1));
            Assert.That(process.IsValid, Is.True);
        }

        /// <summary>
        /// SceneStorageのUnloadで保持中ハンドルが解放されることを検証
        /// </summary>
        [Test]
        public void SimpleSceneStorage_Unload_ReleasesCachedHandle() {
            var loader = new FakeSceneLoader();
            loader.Register("scene");

            using var storage = new SimpleSceneStorage(loader);
            _ = storage.LoadAsync(new SceneRequest("scene", activateOnLoad: false));

            storage.Unload(new SceneRequest("scene", activateOnLoad: false));

            Assert.That(loader.GetReleaseCount("scene"), Is.EqualTo(1));
        }

        /// <summary>
        /// テスト用アセット
        /// </summary>
        private sealed class TestAsset : ScriptableObject {
        }

        /// <summary>
        /// テスト用AssetRequest
        /// </summary>
        private readonly struct TestAssetRequest : IAssetRequest<TestAsset> {
            /// <summary>読み込み対象アドレス</summary>
            public string Address { get; }
            /// <summary>有効な要求か</summary>
            public bool IsValid => !string.IsNullOrEmpty(Address);

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public TestAssetRequest(string address) {
                Address = address;
            }
        }

        /// <summary>
        /// テスト用AssetLoader
        /// </summary>
        private sealed class FakeAssetLoader : IAssetLoader {
            private readonly Dictionary<string, TestAsset> _assets = new();
            private readonly Dictionary<string, FakeAssetLoadHandle<TestAsset>> _handles = new();

            public int LoadCount { get; private set; }

            public void Register(string address, TestAsset asset) {
                _assets[address] = asset;
            }

            public int GetReleaseCount(string address) {
                return _handles.TryGetValue(address, out var handle) ? handle.ReleaseCount : 0;
            }

            public bool CanLoad<TAsset>(IAssetRequest<TAsset> request)
                where TAsset : Object {
                return typeof(TAsset) == typeof(TestAsset) && request.IsValid && _assets.ContainsKey(request.Address);
            }

            public IAssetLoadHandle<TAsset> LoadAsync<TAsset>(IAssetRequest<TAsset> request)
                where TAsset : Object {
                LoadCount++;

                var handle = new FakeAssetLoadHandle<TAsset>(_assets[request.Address] as TAsset);
                _handles[request.Address] = (FakeAssetLoadHandle<TestAsset>)(object)handle;
                return handle;
            }
        }

        /// <summary>
        /// テスト用AssetLoadHandle
        /// </summary>
        private sealed class FakeAssetLoadHandle<TAsset> : IAssetLoadHandle<TAsset>
            where TAsset : Object {
            private bool _isReleased;

            public int ReleaseCount { get; private set; }
            public bool IsDone => true;
            public TAsset Asset => _isReleased ? null : Result;
            public bool IsValid => !_isReleased && Result != null;
            public Exception Exception => null;
            public TAsset Result { get; }
            public object Current => null;

            public FakeAssetLoadHandle(TAsset asset) {
                Result = asset;
            }

            public void Release() {
                if (_isReleased) {
                    return;
                }

                _isReleased = true;
                ReleaseCount++;
            }

            public void Dispose() {
                Release();
            }

            public bool MoveNext() {
                return false;
            }

            public void Reset() {
            }
        }

        /// <summary>
        /// テスト用SceneLoader
        /// </summary>
        private sealed class FakeSceneLoader : ISceneLoader {
            private readonly HashSet<string> _addresses = new();
            private readonly Dictionary<string, FakeSceneLoadHandle> _handles = new();

            public int LoadCount { get; private set; }

            public void Register(string address) {
                _addresses.Add(address);
            }

            public int GetActivateCount(string address) {
                return _handles.TryGetValue(address, out var handle) ? handle.ActivateCount : 0;
            }

            public int GetReleaseCount(string address) {
                return _handles.TryGetValue(address, out var handle) ? handle.ReleaseCount : 0;
            }

            public bool CanLoad(ISceneRequest request) {
                return request.IsValid && _addresses.Contains(request.Address);
            }

            public ISceneLoadHandle LoadAsync(ISceneRequest request) {
                LoadCount++;

                var handle = new FakeSceneLoadHandle();
                _handles[request.Address] = handle;
                return handle;
            }
        }

        /// <summary>
        /// テスト用SceneLoadHandle
        /// </summary>
        private sealed class FakeSceneLoadHandle : ISceneLoadHandle {
            private bool _isReleased;

            public int ActivateCount { get; private set; }
            public int ReleaseCount { get; private set; }
            public bool IsDone => true;
            public Scene Scene => default;
            public bool IsValid => !_isReleased;
            public Exception Exception => null;
            public Scene Result => Scene;
            public object Current => null;

            public AsyncOperationHandle ActivateAsync() {
                ActivateCount++;
                return AsyncOperationHandle.CompletedHandle;
            }

            public void Release() {
                if (_isReleased) {
                    return;
                }

                _isReleased = true;
                ReleaseCount++;
            }

            public void Dispose() {
                Release();
            }

            public bool MoveNext() {
                return false;
            }

            public void Reset() {
            }
        }
    }
}
