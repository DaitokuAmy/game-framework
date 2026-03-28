using System;
using GameFramework.TweenSystem;
using NUnit.Framework;

namespace GameFramework.Tests {
    /// <summary>
    /// TweenSystem の回帰テスト
    /// </summary>
    public class TweenSystemTests {
        /// <summary>
        /// 正常完了した Handle は回収後も完了扱いを維持することを検証
        /// </summary>
        [Test]
        public void CompletedHandle_ShouldRemainCompletedAfterAutoKillCleanup() {
            using var player = new TweenPlayer();

            var handle = player.Play(player.Delay(0.1f));
            player.Tick(0.1f);

            Assert.That(handle.IsValid, Is.False);
            Assert.That(handle.IsCompleted, Is.True);
        }

        /// <summary>
        /// Cancel 済み Handle が完了扱いにならないことを検証
        /// </summary>
        [Test]
        public void CanceledHandle_ShouldNotReportCompleted() {
            using var player = new TweenPlayer();

            var handle = player.Play(player.Delay(0.1f));
            handle.Cancel();

            Assert.That(handle.IsValid, Is.False);
            Assert.That(handle.IsCompleted, Is.False);
        }

        /// <summary>
        /// AutoKill 無効時でも完了後に Cancel で回収できることを検証
        /// </summary>
        [Test]
        public void AutoKillFalseTween_ShouldStayValidUntilCanceled() {
            using var player = new TweenPlayer();

            var tween = player.Delay(0.1f).SetAutoKill(false);
            var handle = player.Play(tween);

            player.Tick(0.1f);

            Assert.That(handle.IsValid, Is.True);
            Assert.That(handle.IsCompleted, Is.True);

            handle.Cancel();

            Assert.That(handle.IsValid, Is.False);
            Assert.That(handle.IsCompleted, Is.False);
        }

        /// <summary>
        /// すでに Sequence に所属している Tween を再追加できないことを検証
        /// </summary>
        [Test]
        public void Append_ShouldRejectTweenOwnedByAnotherSequence() {
            using var player = new TweenPlayer();

            var child = player.Delay(0.1f);
            var first = player.CreateSequence();
            var second = player.CreateSequence();

            first.Append(child);

            Assert.Throws<InvalidOperationException>(() => second.Append(child));
        }

        /// <summary>
        /// Sequence に所属する Tween を単体再生できないことを検証
        /// </summary>
        [Test]
        public void Play_ShouldRejectTweenOwnedBySequence() {
            using var player = new TweenPlayer();

            var child = player.Delay(0.1f);
            player.CreateSequence()
                .Append(child);

            Assert.Throws<InvalidOperationException>(() => player.Play(child));
        }

        /// <summary>
        /// 再生開始後の Sequence 変更を拒否することを検証
        /// </summary>
        [Test]
        public void Append_ShouldRejectMutationAfterPlaybackStarted() {
            using var player = new TweenPlayer();

            var sequence = player.CreateSequence()
                .Append(player.Delay(0.1f));

            player.Play(sequence);

            Assert.Throws<InvalidOperationException>(() => sequence.Append(player.Delay(0.1f)));
        }

        /// <summary>
        /// Cancel 直後に再利用した ID が二重更新されないことを検証
        /// </summary>
        [Test]
        public void Cancel_ShouldNotTickReusedIdTwice() {
            using var player = new TweenPlayer();

            var firstHandle = player.Play(player.Delay(1.0f));
            firstHandle.Cancel();

            var secondHandle = player.Play(player.Delay(0.2f));
            player.Tick(0.1f);

            Assert.That(secondHandle.IsValid, Is.True);
            Assert.That(secondHandle.IsCompleted, Is.False);
        }

        /// <summary>
        /// ForceComplete 直後に再利用した ID が二重更新されないことを検証
        /// </summary>
        [Test]
        public void ForceComplete_ShouldNotTickReusedIdTwice() {
            using var player = new TweenPlayer();

            var firstHandle = player.Play(player.Delay(1.0f));
            firstHandle.ForceComplete();

            var secondHandle = player.Play(player.Delay(0.2f));
            player.Tick(0.1f);

            Assert.That(secondHandle.IsValid, Is.True);
            Assert.That(secondHandle.IsCompleted, Is.False);
        }
    }
}
