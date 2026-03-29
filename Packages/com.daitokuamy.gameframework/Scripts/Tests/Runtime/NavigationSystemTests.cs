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
        private const int LeftSessionNodeId = 4;
        private const int LeftScreenNodeId = 5;
        private const int RightSessionNodeId = 6;
        private const int RightScreenNodeId = 7;

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
        /// Load / Initialize / Release を計測する RootNode
        /// </summary>
        public sealed class LifecycleTrackingRootNode : RootNode {
            public static int StandbyCount { get; private set; }
            public static int LoadCount { get; private set; }
            public static int InitializeCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
                LoadCount = 0;
                InitializeCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                LoadCount++;
                yield break;
            }

            protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                InitializeCount++;
                yield break;
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// Load / Initialize / Release を計測する SessionNode
        /// </summary>
        public sealed class LifecycleTrackingSessionNode : SessionNode {
            public static int StandbyCount { get; private set; }
            public static int LoadCount { get; private set; }
            public static int InitializeCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
                LoadCount = 0;
                InitializeCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                LoadCount++;
                yield break;
            }

            protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                InitializeCount++;
                yield break;
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// Load / Initialize / Activate / Release を計測する ScreenNode
        /// </summary>
        public sealed class LifecycleTrackingScreenNode : ScreenNode {
            public static LifecycleTrackingScreenNode Current { get; private set; }

            public static int StandbyCount { get; private set; }
            public static int LoadCount { get; private set; }
            public static int InitializeCount { get; private set; }
            public static int ActivateCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public LifecycleTrackingScreenNode() {
                Current = this;
            }

            public static void ResetState() {
                Current = null;
                StandbyCount = 0;
                LoadCount = 0;
                InitializeCount = 0;
                ActivateCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
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

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// Load が止まる ScreenNode
        /// </summary>
        public sealed class BlockingLifecycleTrackingScreenNode : ScreenNode {
            public static BlockingLifecycleTrackingScreenNode Current { get; private set; }

            public static int StandbyCount { get; private set; }
            public static int LoadCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public BlockingLifecycleTrackingScreenNode() {
                Current = this;
            }

            public static void ResetState() {
                Current = null;
                StandbyCount = 0;
                LoadCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                LoadCount++;
                while (true) {
                    yield return null;
                }
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// Standby 呼び出し回数を計測する RootNode
        /// </summary>
        public sealed class StandbyTrackingRootNode : RootNode {
            public static int StandbyCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }
        }

        /// <summary>
        /// Standby 呼び出し回数を計測する SessionNode
        /// </summary>
        public sealed class StandbyTrackingSessionNode : SessionNode {
            public static int StandbyCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }
        }

        /// <summary>
        /// Standby 呼び出し回数を計測する ScreenNode
        /// </summary>
        public sealed class StandbyTrackingScreenNode : ScreenNode {
            public static int StandbyCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }
        }

        /// <summary>
        /// shared path の Standby / Release を計測する
        /// </summary>
        public sealed class SharedReleaseRootNode : RootNode {
            public static int StandbyCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// 左側 SessionNode の Standby / Release を計測する
        /// </summary>
        public sealed class LeftReleaseSessionNode : SessionNode {
            public static int StandbyCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// 左側 ScreenNode の Standby / Release を計測する
        /// </summary>
        public sealed class LeftReleaseScreenNode : ScreenNode {
            public static int StandbyCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// 右側 SessionNode の Standby / Release を計測する
        /// </summary>
        public sealed class RightReleaseSessionNode : SessionNode {
            public static int StandbyCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// 右側 ScreenNode の Standby / Release を計測する
        /// </summary>
        public sealed class RightReleaseScreenNode : ScreenNode {
            public static int StandbyCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public static void ResetState() {
                StandbyCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
            }

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// Update回数を計測するScreenNode
        /// </summary>
        public sealed class TrackingScreenNode : ScreenNode {
            public static TrackingScreenNode Current { get; private set; }

            public static int StandbyCount { get; private set; }
            public int LoadCount { get; private set; }
            public int InitializeCount { get; private set; }
            public int ActivateCount { get; private set; }
            public int UpdateAlwaysCount { get; private set; }
            public int UpdateCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public TrackingScreenNode() {
                Current = this;
            }

            public static void ResetState() {
                Current = null;
                StandbyCount = 0;
                ReleaseCount = 0;
            }

            protected override void Standby(IScope scope) {
                StandbyCount++;
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

            protected override void Release() {
                ReleaseCount++;
            }
        }

        /// <summary>
        /// 解除されるまで読み込みが終わらないScreenNode
        /// </summary>
        public sealed class BlockingLoadScreenNode : ScreenNode {
            public static BlockingLoadScreenNode Current { get; private set; }

            public static int LoadCount { get; private set; }
            public static int ReleaseCount { get; private set; }

            public BlockingLoadScreenNode() {
                Current = this;
            }

            public static void ResetState() {
                Current = null;
                LoadCount = 0;
                ReleaseCount = 0;
            }

            protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
                LoadCount++;
                while (true) {
                    yield return null;
                }
            }

            protected override void Release() {
                ReleaseCount++;
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
            StandbyTrackingRootNode.ResetState();
            StandbyTrackingSessionNode.ResetState();
            StandbyTrackingScreenNode.ResetState();
            LifecycleTrackingRootNode.ResetState();
            LifecycleTrackingSessionNode.ResetState();
            LifecycleTrackingScreenNode.ResetState();
            BlockingLifecycleTrackingScreenNode.ResetState();
            SharedReleaseRootNode.ResetState();
            LeftReleaseSessionNode.ResetState();
            LeftReleaseScreenNode.ResetState();
            RightReleaseSessionNode.ResetState();
            RightReleaseScreenNode.ResetState();
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
        /// Build しただけでは Standby されないことを検証
        /// </summary>
        [Test]
        public void Build_ShouldNotStandbyNodesUntilTheyAreUsed() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBuilder<StandbyTrackingRootNode, StandbyTrackingSessionNode, StandbyTrackingScreenNode>().Build();

                Assert.That(StandbyTrackingRootNode.StandbyCount, Is.EqualTo(0));
                Assert.That(StandbyTrackingSessionNode.StandbyCount, Is.EqualTo(0));
                Assert.That(StandbyTrackingScreenNode.StandbyCount, Is.EqualTo(0));
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
                engine = CreateBuilder<LifecycleTrackingRootNode, LifecycleTrackingSessionNode, BlockingLifecycleTrackingScreenNode>().Build();

                var handle = engine.PreLoad(ScreenNodeId);
                engine.Update();

                Assert.That(BlockingLifecycleTrackingScreenNode.LoadCount, Is.EqualTo(1));
                Assert.That(handle.IsDone, Is.False);

                engine.Dispose();
                engine = null;

                Assert.That(handle.IsDone, Is.True);
                Assert.That(handle.Exception, Is.TypeOf<OperationCanceledException>());
                Assert.That(LifecycleTrackingRootNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingSessionNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(BlockingLifecycleTrackingScreenNode.ReleaseCount, Is.EqualTo(1));
            }
            finally {
                engine?.Dispose();
            }
        }

        /// <summary>
        /// 進行中の遷移がDispose/Shutdownで安全に中断されることを検証
        /// </summary>
        [Test]
        public void Dispose_ShouldAbortPendingTransitionHandle() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBuilder<LifecycleTrackingRootNode, LifecycleTrackingSessionNode, BlockingLifecycleTrackingScreenNode>().Build();

                var handle = engine.TransitionTo<BlockingLifecycleTrackingScreenNode>(ScreenNodeId, new DelayedActivationTransition());
                engine.Update();

                Assert.That(BlockingLifecycleTrackingScreenNode.LoadCount, Is.EqualTo(1));
                Assert.That(handle.IsDone, Is.False);
                Assert.That(LifecycleTrackingRootNode.ReleaseCount, Is.EqualTo(0));
                Assert.That(LifecycleTrackingSessionNode.ReleaseCount, Is.EqualTo(0));
                Assert.That(BlockingLifecycleTrackingScreenNode.ReleaseCount, Is.EqualTo(0));

                engine.Dispose();
                engine = null;

                Assert.That(handle.IsDone, Is.True);
                Assert.That(handle.Exception, Is.TypeOf<OperationCanceledException>());
                Assert.That(LifecycleTrackingRootNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingSessionNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(BlockingLifecycleTrackingScreenNode.ReleaseCount, Is.EqualTo(1));
            }
            finally {
                engine?.Dispose();
            }
        }

        /// <summary>
        /// PreLoad で親の Load / Initialize に依存しないことを検証
        /// </summary>
        [Test]
        public void PreLoad_ShouldStandbyPathLoadTargetOnlyAndSkipInitialize() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBuilder<LifecycleTrackingRootNode, LifecycleTrackingSessionNode, LifecycleTrackingScreenNode>().Build();

                var handle = engine.PreLoad(ScreenNodeId);
                engine.Update();

                Assert.That(LifecycleTrackingRootNode.StandbyCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingSessionNode.StandbyCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingScreenNode.StandbyCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingRootNode.LoadCount, Is.EqualTo(0));
                Assert.That(LifecycleTrackingRootNode.InitializeCount, Is.EqualTo(0));
                Assert.That(LifecycleTrackingSessionNode.LoadCount, Is.EqualTo(0));
                Assert.That(LifecycleTrackingSessionNode.InitializeCount, Is.EqualTo(0));
                Assert.That(LifecycleTrackingScreenNode.LoadCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingScreenNode.InitializeCount, Is.EqualTo(0));
                Assert.That(handle.IsDone, Is.True);

                engine.UnPreLoad(ScreenNodeId);
                Assert.That(LifecycleTrackingRootNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingSessionNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(LifecycleTrackingScreenNode.ReleaseCount, Is.EqualTo(1));
            }
            finally {
                engine?.Dispose();
            }
        }

        /// <summary>
        /// 共有親を持つ複数 PreLoad で親が保持されることを検証
        /// </summary>
        [Test]
        public void PreLoad_ShouldKeepSharedParentUntilAllBranchesAreReleased() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBranchingBuilder<SharedReleaseRootNode, LeftReleaseSessionNode, LeftReleaseScreenNode, RightReleaseSessionNode, RightReleaseScreenNode>().Build();

                engine.PreLoad(LeftScreenNodeId);
                engine.Update();
                engine.PreLoad(RightScreenNodeId);
                engine.Update();

                Assert.That(SharedReleaseRootNode.StandbyCount, Is.EqualTo(1));
                Assert.That(LeftReleaseSessionNode.StandbyCount, Is.EqualTo(1));
                Assert.That(LeftReleaseScreenNode.StandbyCount, Is.EqualTo(1));
                Assert.That(RightReleaseSessionNode.StandbyCount, Is.EqualTo(1));
                Assert.That(RightReleaseScreenNode.StandbyCount, Is.EqualTo(1));

                engine.UnPreLoad(LeftScreenNodeId);

                Assert.That(SharedReleaseRootNode.ReleaseCount, Is.EqualTo(0));
                Assert.That(LeftReleaseSessionNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(LeftReleaseScreenNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(RightReleaseSessionNode.ReleaseCount, Is.EqualTo(0));
                Assert.That(RightReleaseScreenNode.ReleaseCount, Is.EqualTo(0));

                engine.UnPreLoad(RightScreenNodeId);

                Assert.That(SharedReleaseRootNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(RightReleaseSessionNode.ReleaseCount, Is.EqualTo(1));
                Assert.That(RightReleaseScreenNode.ReleaseCount, Is.EqualTo(1));
            }
            finally {
                engine?.Dispose();
            }
        }

        /// <summary>
        /// Running 中の Node を UnPreLoad しても Release されないことを検証
        /// </summary>
        [Test]
        public void UnPreLoad_ShouldNotReleaseRunningNodePath() {
            var engine = default(NavigationEngine);
            try {
                engine = CreateBuilder<LifecycleTrackingRootNode, LifecycleTrackingSessionNode, LifecycleTrackingScreenNode>().Build();

                engine.PreLoad(ScreenNodeId);
                engine.Update();

                var handle = engine.TransitionTo<LifecycleTrackingScreenNode>(ScreenNodeId, new DelayedActivationTransition());
                engine.Update();
                engine.Update();

                Assert.That(handle.IsDone, Is.True);

                engine.UnPreLoad(ScreenNodeId);

                Assert.That(LifecycleTrackingRootNode.ReleaseCount, Is.EqualTo(0));
                Assert.That(LifecycleTrackingSessionNode.ReleaseCount, Is.EqualTo(0));
                Assert.That(LifecycleTrackingScreenNode.ReleaseCount, Is.EqualTo(0));
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
            return CreateBuilder<TestRootNode, TestSessionNode, TScreen>();
        }

        /// <summary>
        /// テスト用のツリー構築
        /// </summary>
        private static NavigationEngineBuilder CreateBuilder<TRoot, TSession, TScreen>()
            where TRoot : RootNode, new()
            where TSession : SessionNode, new()
            where TScreen : ScreenNode, new() {
            return NavigationEngineBuilder.Create()
                .CreateLifecycle<TRoot>(RootNodeId, root =>
                    root.AddSession<TSession>(SessionNodeId, session =>
                        session.AddScreen<TScreen>(ScreenNodeId)));
        }

        /// <summary>
        /// 2系統の branch を持つテスト用のツリー構築
        /// </summary>
        private static NavigationEngineBuilder CreateBranchingBuilder<TRoot, TLeftSession, TLeftScreen, TRightSession, TRightScreen>()
            where TRoot : RootNode, new()
            where TLeftSession : SessionNode, new()
            where TLeftScreen : ScreenNode, new()
            where TRightSession : SessionNode, new()
            where TRightScreen : ScreenNode, new() {
            return NavigationEngineBuilder.Create()
                .CreateLifecycle<TRoot>(RootNodeId, root =>
                    root.AddSession<TLeftSession>(LeftSessionNodeId, left =>
                            left.AddScreen<TLeftScreen>(LeftScreenNodeId))
                        .AddSession<TRightSession>(RightSessionNodeId, right =>
                            right.AddScreen<TRightScreen>(RightScreenNodeId)));
        }
    }
}
