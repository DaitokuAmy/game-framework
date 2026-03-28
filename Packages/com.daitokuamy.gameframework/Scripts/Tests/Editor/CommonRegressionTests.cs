using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace GameFramework.Tests {
    /// <summary>
    /// Common 配下の回帰テスト
    /// </summary>
    public sealed class CommonRegressionTests {
        /// <summary>
        /// 初回遷移前でも CheckTransition が安全に判定できることを検証
        /// </summary>
        [Test]
        public void StateTreeRouter_CheckTransitionBeforeFirstTransition_ReturnsExpectedValue() {
            using var stateContainer = new TestStateContainer();
            using var router = new StateTreeRouter<string, object, object>(stateContainer);

            router.ConnectRoot("Root");

            Assert.DoesNotThrow(() => router.CheckTransition("Missing"));
            Assert.That(router.CheckTransition("Missing"), Is.False);
            Assert.That(router.CheckTransition("Root"), Is.True);
        }

        /// <summary>
        /// null キー遷移が例外ハンドルで失敗することを検証
        /// </summary>
        [Test]
        public void StateStackRouter_TransitionToNullKey_ReturnsArgumentNullException() {
            using var stateContainer = new TestStateContainer();
            stateContainer.Register("StateA", new object());
            using var router = new StateStackRouter<string, object, object>(stateContainer);

            var handle = default(TransitionHandle<object>);

            Assert.DoesNotThrow(() => handle = router.TransitionTo(null));
            Assert.That(handle.Exception, Is.TypeOf<ArgumentNullException>());
            Assert.That(stateContainer.TransitionCount, Is.EqualTo(0));
            Assert.That(router.CurrentKey, Is.Null);
        }

        /// <summary>
        /// 追加時キャンセルで待機中の低優先度コマンドが破棄されることを検証
        /// </summary>
        [Test]
        public void CommandManager_AddingHighPriorityCanceler_DestroysLowerPriorityStandbyCommands() {
            using var manager = new CommandManager();

            var lowPriorityCommand = new TestCommand(priority: 1);
            var cancelerCommand = new TestCommand(priority: 10, addedCancelLowPriorityOthers: true);

            manager.Add(lowPriorityCommand);
            manager.Add(cancelerCommand);

            Assert.That(lowPriorityCommand.CurrentState, Is.EqualTo(CommandState.Destroyed));
            Assert.That(cancelerCommand.CurrentState, Is.EqualTo(CommandState.Standby));
        }

        /// <summary>
        /// 実行時キャンセルで実行中の低優先度コマンドが破棄されることを検証
        /// </summary>
        [Test]
        public void CommandManager_ExecutingHighPriorityCanceler_DestroysLowerPriorityExecutingCommands() {
            using var manager = new CommandManager();

            var lowPriorityCommand = new TestCommand(priority: 1, keepRunning: true);
            var cancelerCommand = new TestCommand(priority: 10, executedCancelLowPriorityOthers: true, keepRunning: true);

            manager.Add(lowPriorityCommand);
            manager.Update();
            manager.Add(cancelerCommand);
            manager.Update();

            Assert.That(lowPriorityCommand.CurrentState, Is.EqualTo(CommandState.Destroyed));
            Assert.That(cancelerCommand.CurrentState, Is.EqualTo(CommandState.Executing));
        }

        /// <summary>
        /// LateUpdatable が解除後に再登録できることを検証
        /// </summary>
        [Test]
        public void UpdateScheduler_LateUpdatable_CanReregisterAfterUnregister() {
            using var scheduler = new UpdateScheduler();
            var lateUpdatable = new TestLateUpdatable();

            scheduler.RegisterLateUpdatable(lateUpdatable, 0);
            scheduler.LateUpdate();
            scheduler.UnregisterLateUpdatable(lateUpdatable);
            scheduler.LateUpdate();

            Assert.That(lateUpdatable.RegisteredCount, Is.EqualTo(1));
            Assert.That(lateUpdatable.UnregisteredCount, Is.EqualTo(1));
            Assert.That(lateUpdatable.UpdateCount, Is.EqualTo(1));

            Assert.DoesNotThrow(() => scheduler.RegisterLateUpdatable(lateUpdatable, 0));

            scheduler.LateUpdate();

            Assert.That(lateUpdatable.RegisteredCount, Is.EqualTo(2));
            Assert.That(lateUpdatable.UpdateCount, Is.EqualTo(2));
        }

        /// <summary>
        /// FixedUpdatable が解除後に再登録できることを検証
        /// </summary>
        [Test]
        public void UpdateScheduler_FixedUpdatable_CanReregisterAfterUnregister() {
            using var scheduler = new UpdateScheduler();
            var fixedUpdatable = new TestFixedUpdatable();

            scheduler.RegisterFixedUpdatable(fixedUpdatable, 0);
            scheduler.FixedUpdate();
            scheduler.UnregisterFixedUpdatable(fixedUpdatable);
            scheduler.FixedUpdate();

            Assert.That(fixedUpdatable.RegisteredCount, Is.EqualTo(1));
            Assert.That(fixedUpdatable.UnregisteredCount, Is.EqualTo(1));
            Assert.That(fixedUpdatable.UpdateCount, Is.EqualTo(1));

            Assert.DoesNotThrow(() => scheduler.RegisterFixedUpdatable(fixedUpdatable, 0));

            scheduler.FixedUpdate();

            Assert.That(fixedUpdatable.RegisteredCount, Is.EqualTo(2));
            Assert.That(fixedUpdatable.UpdateCount, Is.EqualTo(2));
        }

        /// <summary>
        /// テスト用の StateContainer
        /// </summary>
        private sealed class TestStateContainer : IStateContainer<string, object, object> {
            private readonly Dictionary<string, object> _states = new();

            public object Current { get; private set; }
            public bool IsTransitioning { get; set; }
            public int TransitionCount { get; private set; }

            public void Dispose() {
            }

            public object FindState(string key) {
                return key != null && _states.TryGetValue(key, out var state) ? state : null;
            }

            public string[] GetStateKeys() {
                var keys = new string[_states.Count];
                _states.Keys.CopyTo(keys, 0);
                return keys;
            }

            public void Register(string key, object state) {
                _states[key] = state;
            }

            public TransitionHandle<object> TransitionTo(string key, object option, bool back, Action<object> setupAction, ITransition transition,
                params ITransitionEffect[] effects) {
                TransitionCount++;
                Current = FindState(key);
                setupAction?.Invoke(Current);
                return TransitionHandle<object>.Empty;
            }

            public TransitionHandle<object> Reset(Action<object> setupAction, params ITransitionEffect[] effects) {
                setupAction?.Invoke(Current);
                return TransitionHandle<object>.Empty;
            }
        }

        /// <summary>
        /// テスト用コマンド
        /// </summary>
        private sealed class TestCommand : Command {
            private readonly bool _keepRunning;

            public override int Priority { get; }
            public override bool AddedCancelLowPriorityOthers { get; }
            public override bool ExecutedCancelLowPriorityOthers { get; }

            public TestCommand(int priority, bool addedCancelLowPriorityOthers = false, bool executedCancelLowPriorityOthers = false, bool keepRunning = false) {
                Priority = priority;
                AddedCancelLowPriorityOthers = addedCancelLowPriorityOthers;
                ExecutedCancelLowPriorityOthers = executedCancelLowPriorityOthers;
                _keepRunning = keepRunning;
            }

            protected override bool UpdateInternal() {
                return _keepRunning;
            }
        }

        /// <summary>
        /// テスト用 LateUpdatable
        /// </summary>
        private sealed class TestLateUpdatable : ILateUpdatable, ILateUpdatableEventHandler {
            public bool IsActive => true;
            public int RegisteredCount { get; private set; }
            public int UnregisteredCount { get; private set; }
            public int UpdateCount { get; private set; }

            public void Update() {
                UpdateCount++;
            }

            public void OnRegistered(UpdateScheduler runner) {
                RegisteredCount++;
            }

            public void OnUnregistered(UpdateScheduler runner) {
                UnregisteredCount++;
            }
        }

        /// <summary>
        /// テスト用 FixedUpdatable
        /// </summary>
        private sealed class TestFixedUpdatable : IFixedUpdatable, IFixedUpdatableEventHandler {
            public bool IsActive => true;
            public int RegisteredCount { get; private set; }
            public int UnregisteredCount { get; private set; }
            public int UpdateCount { get; private set; }

            public void Update() {
                UpdateCount++;
            }

            public void OnRegistered(UpdateScheduler runner) {
                RegisteredCount++;
            }

            public void OnUnregistered(UpdateScheduler runner) {
                UnregisteredCount++;
            }
        }
    }
}
