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
        /// 現在再生中の Playable を取得
        /// </summary>
        private static Playable? GetCurrentPlayable(MotionHandle handle) {
            var crossFaderField = typeof(MotionHandle).GetField("_crossFader", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(crossFaderField, Is.Not.Null);

            var crossFader = crossFaderField.GetValue(handle);
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
    }
}
