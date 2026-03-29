using System.Reflection;
using GameFramework.PlayableSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.TestTools;

namespace GameFramework.Tests {
    /// <summary>
    /// PlayableSystem の回帰テスト
    /// </summary>
    public sealed class PlayableSystemTests {
        private GameObject _motionObject;
        private Animator _animator;
        private GameObject _timelineObject;
        private PlayableDirector _playableDirector;

        /// <summary>
        /// テスト前に Playable 用オブジェクトを初期化
        /// </summary>
        [SetUp]
        public void SetUp() {
            _motionObject = new GameObject("PlayableSystemTests.Motion");
            _animator = _motionObject.AddComponent<Animator>();

            _timelineObject = new GameObject("PlayableSystemTests.Timeline");
            _playableDirector = _timelineObject.AddComponent<PlayableDirector>();
        }

        /// <summary>
        /// テスト後に生成物を破棄
        /// </summary>
        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_motionObject);
            Object.DestroyImmediate(_timelineObject);
        }

        /// <summary>
        /// null Timeline は空ハンドルで無視されることを検証
        /// </summary>
        [Test]
        public void TimelinePlayer_PlayNull_ReturnsEmptyHandle() {
            var player = new TimelinePlayer(_playableDirector, DirectorUpdateMode.Manual);

            var handle = player.Play(null);
            var process = (IProcess)handle;

            Assert.That(process.IsDone, Is.True);
            Assert.That(process.Exception, Is.Null);
        }

        /// <summary>
        /// 現在再生中の Playable が Update 後も維持されることを検証
        /// </summary>
        [Test]
        public void MotionPlayer_Update_KeepCurrentPlayableAlive() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var clip = new AnimationClip();

            var playable = player.Handle.Change(clip, 0.0f, false);
            Assert.That(playable.IsValid(), Is.True);

            player.Update();

            var currentPlayable = GetCurrentPlayable(player.Handle);
            Assert.That(currentPlayable.HasValue, Is.True);
            Assert.That(currentPlayable != null && currentPlayable.Value.IsValid(), Is.True);
        }

        /// <summary>
        /// フェードアウト中の Playable を再利用しても再生時間が維持されることを検証
        /// </summary>
        [Test]
        public void MotionPlayer_ReusingFadedOutPlayable_PreservesPlaybackTime() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var firstClip = new AnimationClip();
            var secondClip = new AnimationClip();
            var graph = GetGraph(player);

            var firstPlayable = player.Handle.Change(firstClip, 0.0f, false);
            graph.Evaluate(0.3f);

            player.Handle.Change(secondClip, 0.2f, false);
            var preservedTime = firstPlayable.GetTime();
            graph.Evaluate(0.1f);
            player.Handle.Change((Playable)firstPlayable, 0.2f, false);
            graph.Evaluate(0.1f);

            Assert.That(firstPlayable.GetTime(), Is.GreaterThan(preservedTime));
        }

        /// <summary>
        /// DSPClock 使用時に DSP 時刻差分で経過時間が算出されることを検証
        /// </summary>
        [Test]
        public void MotionPlayer_GetElapsedTimeWithDspClock_UsesDspElapsedTime() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.DSPClock);
            var currentDspTime = AudioSettings.dspTime;

            SetClockTimeSample(player, currentDspTime - 0.25, true);
            var elapsedTime = GetElapsedTime(player, DirectorUpdateMode.DSPClock);

            Assert.That(elapsedTime, Is.EqualTo(0.25f).Within(0.05f));
        }

        /// <summary>
        /// 更新モード切り替え時に未反映時間が先に flush されることを検証
        /// </summary>
        [Test]
        public void MotionPlayer_SetUpdateMode_FlushesPendingTimeBeforeSwitch() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var clip = new AnimationClip();
            var playable = player.Handle.Change(clip, 0.0f, false);

            SetPendingDeltas(player, 0.25f, 0.25f);
            player.SetUpdateMode(DirectorUpdateMode.GameTime);

            Assert.That(playable.GetTime(), Is.EqualTo(0.25).Within(0.0001));
        }

        /// <summary>
        /// 未反映時間の flush 時に speed が二重適用されないことを検証
        /// </summary>
        [Test]
        public void MotionPlayer_SetUpdateMode_FlushesPendingTimeWithoutDoubleApplyingSpeed() {
            using var expectedPlayer = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            using var actualPlayer = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var clip = new AnimationClip();

            expectedPlayer.SetSpeed(2.0f);
            actualPlayer.SetSpeed(2.0f);

            var expectedPlayable = expectedPlayer.Handle.Change(clip, 0.0f, false);
            var expectedGraph = GetGraph(expectedPlayer);
            expectedGraph.Evaluate(0.25f);

            var actualPlayable = actualPlayer.Handle.Change(clip, 0.0f, false);
            SetPendingDeltas(actualPlayer, 0.5f, 0.25f);
            actualPlayer.SetUpdateMode(DirectorUpdateMode.GameTime);

            Assert.That(actualPlayable.GetTime(), Is.EqualTo(expectedPlayable.GetTime()).Within(0.0001));
        }

        /// <summary>
        /// Speed 変更時に未反映時間が旧 speed のまま flush されることを検証
        /// </summary>
        [Test]
        public void MotionPlayer_SetSpeed_FlushesPendingTimeBeforeChangingSpeed() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var clip = new AnimationClip();
            var playable = player.Handle.Change(clip, 0.0f, false);

            SetPendingDeltas(player, 0.25f, 0.25f);
            player.SetSpeed(2.0f);

            Assert.That(playable.GetTime(), Is.EqualTo(0.25).Within(0.0001));
        }

        /// <summary>
        /// 手前の拡張レイヤー削除後も後続レイヤーのウェイト変更が維持されることを検証
        /// </summary>
        [Test]
        public void MotionPlayer_RemoveEarlierExtensionLayer_KeepsLaterLayerWeightAccessible() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);

            var firstLayer = player.CreateExtensionLayer(weight: 0.25f);
            var secondLayer = player.CreateExtensionLayer(weight: 0.75f);

            Assert.That(secondLayer.GetWeight(), Is.EqualTo(0.75f).Within(0.0001f));

            player.RemoveExtensionLayer(firstLayer);
            secondLayer.SetWeight(0.5f);

            Assert.That(secondLayer.GetWeight(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        /// <summary>
        /// 削除済みレイヤーのハンドルが安全に無効化されることを検証
        /// </summary>
        [Test]
        public void MotionPlayer_RemovedLayerHandle_IsSafeToUse() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);

            var layer = player.CreateExtensionLayer(weight: 0.4f);
            player.RemoveExtensionLayer(layer);

            Assert.That(layer.GetWeight(), Is.EqualTo(0.0f));
            Assert.DoesNotThrow(() => layer.SetWeight(1.0f));
            Assert.DoesNotThrow(() => layer.Dispose());
        }

        /// <summary>
        /// AnimationJobConnector の speed 変更が既存 Playable に反映されることを検証
        /// </summary>
        [Test]
        public void AnimationJobConnector_SetSpeed_UpdatesExistingPlayableSpeed() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var component = new RootAnimationJobComponent();

            player.JobConnector.AddComponent(component);
            player.JobConnector.SetSpeed(2.0f);

            var playable = ((IAnimationJobComponent)component).GetPlayable();
            Assert.That(playable.IsValid(), Is.True);
            Assert.That(playable.GetSpeed(), Is.EqualTo(2.0f).Within(0.0001));
        }

        /// <summary>
        /// 無効な AnimationJobComponent が追加されても安全に拒否されることを検証
        /// </summary>
        [Test]
        public void AnimationJobConnector_AddInvalidComponent_DisposesRejectedComponent() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var component = new InvalidAnimationJobComponent();

            LogAssert.Expect(LogType.Error, $"Failed to initialize animation job component. {component}");
            Assert.DoesNotThrow(() => player.JobConnector.AddComponent(component));
            Assert.DoesNotThrow(() => player.Update());

            Assert.That(((IAnimationJobComponent)component).IsInitialized, Is.False);
            Assert.That(component.DisposeCount, Is.EqualTo(1));
        }

        /// <summary>
        /// 追加済み AnimationJobComponent が無効化されたときに破棄されることを検証
        /// </summary>
        [Test]
        public void AnimationJobConnector_InvalidatedComponent_DisposesDroppedComponent() {
            using var player = new MotionPlayer(_animator, DirectorUpdateMode.Manual);
            var component = new DisposableInvalidatedAnimationJobComponent();

            player.JobConnector.AddComponent(component);
            component.Invalidate();

            Assert.DoesNotThrow(() => player.Update());
            Assert.That(((IAnimationJobComponent)component).IsDisposed, Is.True);
            Assert.That(component.DisposeCount, Is.EqualTo(1));
        }

        /// <summary>
        /// 現在再生中の Playable を取得
        /// </summary>
        private static Playable? GetCurrentPlayable(MotionHandle handle) {
            var crossFader = GetCrossFader(handle);
            Assert.That(crossFader, Is.Not.Null);

            var currentInfoField = crossFader.GetType().GetField("_currentPlayingInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(currentInfoField, Is.Not.Null);

            var currentInfo = currentInfoField.GetValue(crossFader);
            Assert.That(currentInfo, Is.Not.Null);

            var playableField = currentInfo.GetType().GetField("Playable", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(playableField, Is.Not.Null);

            return (Playable?)playableField.GetValue(currentInfo);
        }

        /// <summary>
        /// ハンドルから MotionCrossFader を取得
        /// </summary>
        private static object GetCrossFader(MotionHandle handle) {
            var crossFaderField = typeof(MotionHandle).GetField("_crossFader", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(crossFaderField, Is.Not.Null);
            return crossFaderField.GetValue(handle);
        }

        /// <summary>
        /// MotionPlayer の Graph を取得
        /// </summary>
        private static PlayableGraph GetGraph(MotionPlayer player) {
            var graphField = typeof(MotionPlayer).GetField("_graph", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(graphField, Is.Not.Null);
            return (PlayableGraph)graphField.GetValue(player);
        }

        /// <summary>
        /// DSPClock 用サンプル状態を設定
        /// </summary>
        private static void SetClockTimeSample(MotionPlayer player, double previousClockTime, bool hasClockTimeSample) {
            var previousClockTimeField = typeof(MotionPlayer).GetField("_previousClockTime", BindingFlags.Instance | BindingFlags.NonPublic);
            var hasClockTimeSampleField = typeof(MotionPlayer).GetField("_hasClockTimeSample", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(previousClockTimeField, Is.Not.Null);
            Assert.That(hasClockTimeSampleField, Is.Not.Null);

            previousClockTimeField.SetValue(player, previousClockTime);
            hasClockTimeSampleField.SetValue(player, hasClockTimeSample);
        }

        /// <summary>
        /// 蓄積済みの時間を設定
        /// </summary>
        private static void SetPendingDeltas(MotionPlayer player, float pendingSimulationDelta, float pendingEvaluateDelta) {
            var pendingSimulationDeltaField = typeof(MotionPlayer).GetField("_pendingSimulationDelta", BindingFlags.Instance | BindingFlags.NonPublic);
            var pendingEvaluateDeltaField = typeof(MotionPlayer).GetField("_pendingEvaluateDelta", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(pendingSimulationDeltaField, Is.Not.Null);
            Assert.That(pendingEvaluateDeltaField, Is.Not.Null);

            pendingSimulationDeltaField.SetValue(player, pendingSimulationDelta);
            pendingEvaluateDeltaField.SetValue(player, pendingEvaluateDelta);
        }

        /// <summary>
        /// 経過時間の取得を直接実行
        /// </summary>
        private static float GetElapsedTime(MotionPlayer player, DirectorUpdateMode updateMode) {
            var getElapsedTimeMethod = typeof(MotionPlayer).GetMethod("GetElapsedTime", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(getElapsedTimeMethod, Is.Not.Null);
            return (float)getElapsedTimeMethod.Invoke(player, new object[] { updateMode });
        }

        /// <summary>
        /// 無効な Playable を返すテスト用 AnimationJobComponent
        /// </summary>
        private sealed class InvalidAnimationJobComponent : AnimationJobComponent {
            public int DisposeCount { get; private set; }

            protected override AnimationScriptPlayable CreatePlayable(Animator animator, PlayableGraph graph) {
                return default;
            }

            protected override void DisposeInternal() {
                DisposeCount++;
            }
        }

        /// <summary>
        /// 追加後に無効化されるテスト用 AnimationJobComponent
        /// </summary>
        private sealed class DisposableInvalidatedAnimationJobComponent : AnimationJobComponent {
            private AnimationScriptPlayable _playable;

            public int DisposeCount { get; private set; }

            public void Invalidate() {
                if (_playable.IsValid()) {
                    _playable.Destroy();
                }
            }

            protected override AnimationScriptPlayable CreatePlayable(Animator animator, PlayableGraph graph) {
                _playable = AnimationScriptPlayable.Create(graph, new EmptyAnimationJob());
                return _playable;
            }

            protected override void DisposeInternal() {
                DisposeCount++;
            }
        }

        /// <summary>
        /// テスト用の空 AnimationJob
        /// </summary>
        private struct EmptyAnimationJob : IAnimationJob {
            void IAnimationJob.ProcessRootMotion(AnimationStream stream) {
            }

            void IAnimationJob.ProcessAnimation(AnimationStream stream) {
            }
        }
    }
}
