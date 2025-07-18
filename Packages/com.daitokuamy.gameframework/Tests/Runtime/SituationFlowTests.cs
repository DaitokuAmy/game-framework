using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using NUnit.Framework;

namespace GameFramework.Tests {
    /// <summary>
    /// SituationFlow を通じての遷移構成・制御・状態遷移を検証するテストクラスです。
    /// </summary>
    public class SituationFlowTests {
        /// <summary>
        /// SituationFlow の接続と遷移制御を検証するテスト用基底クラスです。
        /// 各種ライフサイクル処理の呼び出し記録が可能です。
        /// </summary>
        private class MockSituationBase : Situation {
            public bool LoadCalled;
            public bool SetupCalled;
            public bool Activated;

            /// <summary>
            /// 読み込み処理（LoadRoutine）の呼び出しを記録します。
            /// </summary>
            protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
                LoadCalled = true;
                yield break;
            }

            /// <summary>
            /// 初期化処理（SetupRoutine）の呼び出しを記録します。
            /// </summary>
            protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
                SetupCalled = true;
                yield break;
            }

            /// <summary>
            /// アクティベート処理（Activate）の呼び出しを記録します。
            /// </summary>
            protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
                Activated = true;
            }
        }

        /// <summary>
        /// テスト用の MockSituationRoot クラスです。
        /// </summary>
        private class MockSituationRoot : MockSituationBase {
        }

        /// <summary>
        /// テスト用の MockSituationA クラスです。
        /// </summary>
        private class MockSituationA : MockSituationBase {
        }

        /// <summary>
        /// テスト用の MockSituationB クラスです。
        /// </summary>
        private class MockSituationB : MockSituationBase {
        }

        /// <summary>
        /// テスト用の MockSituationC クラスです。
        /// </summary>
        private class MockSituationC : MockSituationBase {
        }

        private CoroutineRunner _runner;
        private SituationContainer _container;
        private SituationFlow _flow;

        /// <summary>
        /// テスト開始前にコンテナとフローを初期化し、接続状態を構築します。
        /// </summary>
        [SetUp]
        public void Setup() {
            _runner = new CoroutineRunner();

            // Situation構築
            _container = new SituationContainer();
            var situationRoot = new MockSituationRoot();
            var situationA = new MockSituationA();
            var situationB = new MockSituationB();
            var situationC = new MockSituationC();
            situationA.SetParent(situationRoot);
            situationB.SetParent(situationRoot);
            situationC.SetParent(situationRoot);
            _container.Setup(situationRoot);

            // ルート接続と遷移関係の構築
            _flow = new SituationFlow(_container);
            var aNode = _flow.ConnectRoot<MockSituationA>();
            var bNode = aNode.Connect<MockSituationB>();
            bNode.Connect<MockSituationC>();
        }

        /// <summary>
        /// テスト終了後にリソースを解放します。
        /// </summary>
        [TearDown]
        public void Teardown() {
            _flow.Dispose();
            _container.Dispose();
            _runner.Dispose();
        }

        /// <summary>
        /// ConnectRoot により SituationContainer にルートが正しく登録されることを検証します。
        /// </summary>
        [Test]
        public void RootIsRegisteredOnConnectRoot() {
            var root = _container.RootSituation;
            Assert.IsNotNull(root);
            Assert.IsInstanceOf<MockSituationRoot>(root);
        }

        /// <summary>
        /// Flow から指定した Situation へ遷移が可能であることを検証します。
        /// </summary>
        [Test]
        public void FlowCanTransitBetweenConnectedSituations() {
            var handle = _flow.Transition<MockSituationA>();
            RunCoroutineUntilDone(() => handle.IsDone);

            var current = _container.Current as MockSituationA;
            Assert.IsNotNull(current);
            Assert.IsTrue(current.LoadCalled);
            Assert.IsTrue(current.SetupCalled);
            Assert.IsTrue(current.Activated);
        }

        /// <summary>
        /// 接続されていない Situation への遷移は失敗することを確認します。
        /// </summary>
        [Test]
        public void FlowTransitionToUnconnectedSituationFails() {
            // 未接続のノードを直接追加してテスト
            var handle = _flow.Transition<MockSituationB>();
            RunCoroutineUntilDone(() => handle.IsDone);

            Assert.IsFalse(handle.IsValid);
            Assert.IsNotNull(handle.Exception);
        }

        /// <summary>
        /// Flow 経由で複数段階の遷移が可能であることを検証します。
        /// </summary>
        [Test]
        public void FlowCanTransitToDeepChild() {
            var handle1 = _flow.Transition<MockSituationA>();
            RunCoroutineUntilDone(() => handle1.IsDone);
            var handle2 = _flow.Transition<MockSituationB>();
            RunCoroutineUntilDone(() => handle2.IsDone);
            var handle3 = _flow.Transition<MockSituationC>();
            RunCoroutineUntilDone(() => handle3.IsDone);

            var current = _container.Current as MockSituationC;
            Assert.IsNotNull(current);
            Assert.IsTrue(current.LoadCalled);
            Assert.IsTrue(current.SetupCalled);
            Assert.IsTrue(current.Activated);
        }

        /// <summary>
        /// Flow 経由で複数段階の遷移後に戻って来れる事を検証します。
        /// </summary>
        [Test]
        public void FlowCanBackToDeepChild() {
            var handle1 = _flow.Transition<MockSituationA>();
            RunCoroutineUntilDone(() => handle1.IsDone);
            var handle2 = _flow.Transition<MockSituationB>();
            RunCoroutineUntilDone(() => handle2.IsDone);
            var handle3 = _flow.Transition<MockSituationC>();
            RunCoroutineUntilDone(() => handle3.IsDone);
            var handle4 = _flow.Back();
            RunCoroutineUntilDone(() => handle4.IsDone);
            var handle5 = _flow.Back();
            RunCoroutineUntilDone(() => handle5.IsDone);

            var current = _container.Current as MockSituationA;
            Assert.IsNotNull(current);
            Assert.IsTrue(current.LoadCalled);
            Assert.IsTrue(current.SetupCalled);
            Assert.IsTrue(current.Activated);
        }

        /// <summary>
        /// コルーチンを一定フレーム進めてすべての遷移処理を完了させます。
        /// </summary>
        /// <param name="checkDoneFunc">完了チェック処理</param>
        /// <param name="maxFrame">最大試行フレーム数（無限ループ防止）</param>
        private void RunCoroutineUntilDone(System.Func<bool> checkDoneFunc, int maxFrame = 1000) {
            for (var i = 0; i < maxFrame; i++) {
                _runner.Update();
                _container.Update();
                _container.LateUpdate();
                _container.FixedUpdate();

                if (checkDoneFunc()) {
                    break;
                }
            }
        }
    }
}