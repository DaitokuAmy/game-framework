using System;
using System.Collections;
using GameFramework.NavigationSystem;
using NUnit.Framework;

namespace GameFramework.Tests {
    /// <summary>
    /// NavigationSystemの回帰テスト
    /// </summary>
    public class NavigationSystemTests {
        private const int RootNodeId = 1;
        private const int SessionNodeId = 2;
        private const int ScreenNodeId = 3;

        /// <summary>
        /// テスト用RootNode
        /// </summary>
        public sealed class TestRootNode : RootNode {
        }

        /// <summary>
        /// テスト用SessionNode
        /// </summary>
        public sealed class TestSessionNode : SessionNode {
        }

        /// <summary>
        /// テスト用ScreenNode
        /// </summary>
        public sealed class TestScreenNode : ScreenNode {
        }

        /// <summary>
        /// Update回数を計測するScreenNode
        /// </summary>
        public sealed class TrackingScreenNode : ScreenNode {
            public static TrackingScreenNode Current { get; private set; }

            public int LoadCount { get; private set; }
            public int InitializeCount { get; private set; }
            public int ActivateCount { get; private set; }
            public int UpdateAlwaysCount { get; private set; }
            public int UpdateCount { get; private set; }

            public TrackingScreenNode() {
                Current = this;
            }

            public static void ResetState() {
                Current = null;
            }

            protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                LoadCount++;
                yield break;
            }

            protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                InitializeCount++;
                yield break;
            }

            protected override void Activate(TransitionHandle<INavNode> handle, IScope scope) {
                ActivateCount++;
            }

            protected override void UpdateAlways() {
                UpdateAlwaysCount++;
            }

            protected override void Update() {
                UpdateCount++;
            }
        }

        /// <summary>
        /// 解除されるまで読み込みが終わらないScreenNode
        /// </summary>
        public sealed class BlockingLoadScreenNode : ScreenNode {
            public static BlockingLoadScreenNode Current { get; private set; }

            public static int LoadCount { get; private set; }

            public BlockingLoadScreenNode() {
                Current = this;
            }

            public static void ResetState() {
                Current = null;
                LoadCount = 0;
            }

            protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                LoadCount++;
                while (true) {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// 遷移完了前に1フレーム待つ遷移
        /// </summary>
        private sealed class DelayedActivationTransition : ITransition {
            IEnumerator ITransition.TransitionRoutine(ITransitionResolver resolver, bool immediate) {
                resolver.Start();
                resolver.DeactivatePrev();
                yield return resolver.LoadNextRoutine();
                yield return null;
                resolver.UnloadPrev();
                resolver.ActivateNext();
                resolver.Finish();
            }
        }

        /// <summary>
        /// テスト前に静的状態を初期化
        /// </summary>
        [SetUp]
        public void SetUp() {
            TrackingScreenNode.ResetState();
            BlockingLoadScreenNode.ResetState();
        }

        /// <summary>
        /// CreateLifecycle未設定のままBuildすると例外になることを検証
        /// </summary>
        [Test]
        public void Build_WithoutCreateLifecycle_ShouldThrowInvalidOperationException() {
            var builder = NavigationEngineBuilder.Create();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        /// <summary>
        /// Router未設定でもBuildできることを検証
        /// </summary>
        [Test]
        public void Build_WithoutRouter_ShouldSucceed() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBuilder<TestScreenNode>().Build();
                Assert.That(engine, Is.Not.Null);
            }
            finally {
                engine?.Dispose();
            }
        }

        /// <summary>
        /// 進行中のPreLoadがDisposeで中断として終端することを検証
        /// </summary>
        [Test]
        public void Dispose_ShouldAbortPendingPreLoadHandle() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBuilder<BlockingLoadScreenNode>().Build();

                var handle = engine.PreLoad(ScreenNodeId);
                engine.Update();

                Assert.That(BlockingLoadScreenNode.LoadCount, Is.EqualTo(1));
                Assert.That(handle.IsDone, Is.False);

                engine.Dispose();
                engine = null;

                Assert.That(handle.IsDone, Is.True);
                Assert.That(handle.Exception, Is.TypeOf<OperationCanceledException>());
            }
            finally {
                engine?.Dispose();
            }
        }

        /// <summary>
        /// UpdateAlwaysとUpdateの呼び分けを検証
        /// </summary>
        [Test]
        public void UpdateAlways_ShouldRunBeforeUpdate_WhenNodeIsNotActiveYet() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBuilder<TrackingScreenNode>().Build();

                var handle = engine.TransitionTo<TrackingScreenNode>(ScreenNodeId, new DelayedActivationTransition());
                var screen = TrackingScreenNode.Current;

                Assert.That(screen, Is.Not.Null);
                Assert.That(((INavNode)screen).IsActive, Is.False);
                Assert.That(screen.LoadCount, Is.EqualTo(0));
                Assert.That(screen.InitializeCount, Is.EqualTo(0));
                Assert.That(screen.ActivateCount, Is.EqualTo(0));
                Assert.That(screen.UpdateAlwaysCount, Is.EqualTo(0));
                Assert.That(screen.UpdateCount, Is.EqualTo(0));

                engine.Update();

                Assert.That(screen.LoadCount, Is.EqualTo(1));
                Assert.That(screen.InitializeCount, Is.EqualTo(1));
                Assert.That(screen.ActivateCount, Is.EqualTo(0));
                Assert.That(screen.UpdateAlwaysCount, Is.EqualTo(1));
                Assert.That(screen.UpdateCount, Is.EqualTo(0));
                Assert.That(((INavNode)screen).IsActive, Is.False);
                Assert.That(handle.IsDone, Is.False);

                engine.Update();

                Assert.That(screen.ActivateCount, Is.EqualTo(1));
                Assert.That(screen.UpdateAlwaysCount, Is.EqualTo(2));
                Assert.That(screen.UpdateCount, Is.EqualTo(1));
                Assert.That(((INavNode)screen).IsActive, Is.True);
                Assert.That(handle.IsDone, Is.True);
            }
            finally {
                engine?.Dispose();
            }
        }

        /// <summary>
        /// テスト用のツリー構築
        /// </summary>
        private static NavigationEngineBuilder CreateBuilder<TScreen>()
            where TScreen : ScreenNode, new() {
            return NavigationEngineBuilder.Create()
                .CreateLifecycle<TestRootNode>(RootNodeId, root =>
                    root.AddSession<TestSessionNode>(SessionNodeId, session =>
                        session.AddScreen<TScreen>(ScreenNodeId)));
        }
    }
}
