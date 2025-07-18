using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using NUnit.Framework;

namespace GameFramework.Tests {
    /// <summary>
    /// SituationContainer の遷移・プリロード挙動を検証するテストクラスです。
    /// </summary>
    public class SituationContainerTests {
        /// <summary>
        /// シチュエーションテスト用の基底モッククラスです。
        /// 遷移処理における Load, Setup, Activate の呼び出し確認に使用されます。
        /// </summary>
        private class MockSituationBase : Situation {
            public bool LoadCalled;
            public bool SetupCalled;
            public bool Activated;

            /// <summary>
            /// 読み込み処理（呼び出し確認用）
            /// </summary>
            protected override IEnumerator LoadRoutineInternal(TransitionHandle handle, IScope scope) {
                LoadCalled = true;
                yield break;
            }

            /// <summary>
            /// 初期化処理（呼び出し確認用）
            /// </summary>
            protected override IEnumerator SetupRoutineInternal(TransitionHandle handle, IScope scope) {
                SetupCalled = true;
                yield break;
            }

            /// <summary>
            /// アクティブ化処理（呼び出し確認用）
            /// </summary>
            protected override void ActivateInternal(TransitionHandle handle, IScope scope) {
                Activated = true;
            }
        }

        /// <summary>
        /// 型分離のための MockSituationA クラスです。
        /// </summary>
        private class MockSituationA : MockSituationBase {
        }

        /// <summary>
        /// 型分離のための MockSituationB クラスです。
        /// </summary>
        private class MockSituationB : MockSituationBase {
        }

        private SituationContainer _container;
        private CoroutineRunner _runner;

        /// <summary>
        /// 各テスト前にコンテナとコルーチンランナーを初期化します。
        /// </summary>
        [SetUp]
        public void Setup() {
            _container = new SituationContainer();
            _runner = new CoroutineRunner();
        }

        /// <summary>
        /// 各テスト後にコンテナとコルーチンランナーを破棄します。
        /// </summary>
        [TearDown]
        public void Teardown() {
            _container.Dispose();
            _runner.Dispose();
        }

        /// <summary>
        /// Setup で指定した RootSituation が正しく登録されるか検証します。
        /// </summary>
        [Test]
        public void SetupRegistersRootSituation() {
            var root = new MockSituationA();
            _container.Setup(root);

            Assert.AreEqual(root, _container.RootSituation);
            Assert.AreSame(_container, root.Container);
        }

        /// <summary>
        /// Transition により Load → Setup → Activate が呼び出されることを検証します。
        /// </summary>
        [Test]
        public void Transition_CallsLifecycleMethods() {
            var situationA = new MockSituationA();
            var situationB = new MockSituationB();
            situationB.SetParent(situationA);
            _container.Setup(situationA);

            var handle = _container.Transition<MockSituationB>();
            RunCoroutineUntilDone(() => handle.IsDone);

            Assert.IsTrue(situationB.LoadCalled, "LoadRoutine was not called");
            Assert.IsTrue(situationB.SetupCalled, "SetupRoutine was not called");
            Assert.IsTrue(situationB.Activated, "Activate was not called");
            Assert.IsTrue(handle.IsDone, "Transition did not complete");
        }

        /// <summary>
        /// 同一の Situation への遷移がキャンセルされ、無効なハンドルとなることを検証します。
        /// </summary>
        [Test]
        public void Transition_SameTarget_IsCanceled() {
            var situationA = new MockSituationA();
            var situationB = new MockSituationB();
            situationB.SetParent(situationA);
            _container.Setup(situationA);

            var handle1 = _container.Transition<MockSituationB>();
            RunCoroutineUntilDone(() => handle1.IsDone);

            var handle2 = _container.Transition<MockSituationB>();
            RunCoroutineUntilDone(() => handle2.IsDone);

            Assert.IsFalse(handle2.IsValid);
            Assert.IsNotNull(handle2.Exception);
            StringAssert.Contains("Cancel", handle2.Exception.Message);
        }

        /// <summary>
        /// PreLoadAsync により PreLoad 状態が PreLoaded に遷移することを検証します。
        /// </summary>
        [Test]
        public void PreLoad_WorksCorrectly() {
            var situationA = new MockSituationA();
            var situationB = new MockSituationB();
            situationB.SetParent(situationA);
            _container.Setup(situationA);

            var handle = _container.PreLoadAsync<MockSituationB>();
            RunCoroutineUntilDone(() => handle.IsDone);

            Assert.AreEqual(PreLoadState.PreLoaded, situationB.PreLoadState);
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