using System;
using System.Collections;
using System.Reflection;
using GameFramework.BootSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GameFramework.Tests {
    /// <summary>
    /// BootSystem の回帰テスト
    /// </summary>
    public sealed class BootSystemTests {
        private GameObject _bootManagerObject;
        private GameObject _starterObject;

        /// <summary>
        /// テスト前に静的状態を初期化
        /// </summary>
        [SetUp]
        public void SetUp() {
            SetBootManagerInstance(null);
            SetStarterCurrent(null);
        }

        /// <summary>
        /// テスト後に生成物を破棄
        /// </summary>
        [TearDown]
        public void TearDown() {
            SetBootManagerInstance(null);
            SetStarterCurrent(null);

            DestroyImmediateIfExists(_bootManagerObject);
            DestroyImmediateIfExists(_starterObject);

            _bootManagerObject = null;
            _starterObject = null;
        }

        /// <summary>
        /// 開始処理失敗でも IsWarming が解除されることを検証
        /// </summary>
        [Test]
        public void MainSystemBase_StartRoutine_WhenStartThrows_ClearsWarming() {
            var mainSystem = CreateMainSystem();
            mainSystem.ThrowOnStart = true;

            Assert.Throws<InvalidOperationException>(() => RunEnumerator(InvokeRoutine(mainSystem, "StartRoutine", Array.Empty<object>())));
            Assert.That(mainSystem.IsWarmingForTest, Is.False);
        }

        /// <summary>
        /// リブート処理失敗でも IsWarming が解除されることを検証
        /// </summary>
        [Test]
        public void MainSystemBase_RebootRoutine_WhenRebootThrows_ClearsWarming() {
            var mainSystem = CreateMainSystem();
            mainSystem.ThrowOnReboot = true;

            Assert.Throws<InvalidOperationException>(() => RunEnumerator(InvokeRoutine(mainSystem, "RebootRoutine", Array.Empty<object>())));
            Assert.That(mainSystem.IsWarmingForTest, Is.False);
        }

        /// <summary>
        /// MainSystem 失敗後も BootManager が Active に戻ることを検証
        /// </summary>
        [Test]
        public void BootManager_StartRoutine_WhenMainSystemThrows_RestoresActiveState() {
            var bootManager = CreateBootManager(out var mainSystem);
            mainSystem.ThrowOnStart = true;

            Assert.Throws<InvalidOperationException>(() => RunEnumerator(InvokeRoutine(bootManager, "StartRoutine", Array.Empty<object>())));
            Assert.That(GetBootManagerState(bootManager), Is.EqualTo("Active"));
        }

        /// <summary>
        /// Starter が null 引数を返しても空配列として起動できることを検証
        /// </summary>
        [Test]
        public void BootManager_Start_WhenStarterReturnsNullArguments_UsesEmptyArray() {
            var starter = CreateStarter();
            starter.Arguments = null;
            SetStarterCurrent(starter);
            var bootManager = CreateBootManager(out var mainSystem);

            RunEnumerator(InvokeRoutine(bootManager, "Start"));

            Assert.That(mainSystem.StartArguments, Is.Not.Null);
            Assert.That(mainSystem.StartArguments.Length, Is.EqualTo(0));
        }

        /// <summary>
        /// BootScene 未設定時はエラーログを出して StartInternal を呼ばないことを検証
        /// </summary>
        [Test]
        public void MainSystemStarter_Start_WithEmptyBootScene_LogsErrorWithoutCallingStartInternal() {
            var starter = CreateStarter();
            SetStarterCurrent(starter);
            string errorMessage = null;
            LogType? errorLogType = null;

            void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
                if (type != LogType.Error) {
                    return;
                }

                errorMessage = condition;
                errorLogType = type;
            }

            Application.logMessageReceived += OnLogMessageReceived;
            try {
                LogAssert.ignoreFailingMessages = true;
                InvokeVoid(starter, "Start");
            }
            finally {
                LogAssert.ignoreFailingMessages = false;
                Application.logMessageReceived -= OnLogMessageReceived;
            }

            Assert.That(errorLogType, Is.EqualTo(LogType.Error));
            Assert.That(errorMessage, Is.EqualTo("Boot scene name is empty."));
            Assert.That(starter.StartInternalCount, Is.EqualTo(0));
        }

        /// <summary>
        /// BootManager とテスト用 MainSystem を生成
        /// </summary>
        private BootManager CreateBootManager(out TestBootMainSystem mainSystem) {
            _bootManagerObject = new GameObject("BootSystemTests.BootManager");
            var bootManager = _bootManagerObject.AddComponent<BootManager>();
            mainSystem = _bootManagerObject.AddComponent<TestBootMainSystem>();
            Assert.That(bootManager, Is.Not.Null);
            Assert.That(mainSystem, Is.Not.Null);

            SetPrivateField(bootManager, "_mainSystem", mainSystem);
            SetBootManagerInstance(bootManager);
            return bootManager;
        }

        /// <summary>
        /// テスト用 MainSystem を生成
        /// </summary>
        private TestBootMainSystem CreateMainSystem() {
            _bootManagerObject = new GameObject("BootSystemTests.MainSystem");
            var mainSystem = _bootManagerObject.AddComponent<TestBootMainSystem>();
            Assert.That(mainSystem, Is.Not.Null);
            return mainSystem;
        }

        /// <summary>
        /// テスト用 Starter を生成
        /// </summary>
        private TestBootMainSystemStarter CreateStarter() {
            _starterObject = new GameObject("BootSystemTests.Starter");
            var starter = _starterObject.AddComponent<TestBootMainSystemStarter>();
            Assert.That(starter, Is.Not.Null);
            return starter;
        }

        /// <summary>
        /// BootManager の現在状態を取得
        /// </summary>
        private static string GetBootManagerState(BootManager bootManager) {
            var field = typeof(BootManager).GetField("_currentState", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return field.GetValue(bootManager)?.ToString();
        }

        /// <summary>
        /// IEnumerator メソッドを反射で起動
        /// </summary>
        private static IEnumerator InvokeRoutine(object target, string methodName, params object[] arguments) {
            var method = FindInstanceMethod(target.GetType(), methodName);
            Assert.That(method, Is.Not.Null);
            return (IEnumerator)method.Invoke(target, NormalizeArguments(method, arguments));
        }

        /// <summary>
        /// void メソッドを反射で起動
        /// </summary>
        private static void InvokeVoid(object target, string methodName, params object[] arguments) {
            var method = FindInstanceMethod(target.GetType(), methodName);
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, NormalizeArguments(method, arguments));
        }

        /// <summary>
        /// 継承階層を含めてインスタンスメソッドを検索
        /// </summary>
        private static MethodInfo FindInstanceMethod(Type type, string methodName) {
            for (var current = type; current != null; current = current.BaseType) {
                var method = current.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (method != null) {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// 反射呼び出し用に引数を正規化
        /// </summary>
        private static object[] NormalizeArguments(MethodInfo method, object[] arguments) {
            var parameters = method.GetParameters();
            if (parameters.Length == 0) {
                return Array.Empty<object>();
            }

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object[])) {
                if (arguments.Length == 1 && arguments[0] is object[] arrayArgument) {
                    return new object[] { arrayArgument };
                }

                return new object[] { arguments };
            }

            return arguments;
        }

        /// <summary>
        /// IEnumerator を最後まで実行
        /// </summary>
        private static void RunEnumerator(IEnumerator routine) {
            try {
                while (routine.MoveNext()) {
                    if (routine.Current is IEnumerator nested) {
                        RunEnumerator(nested);
                    }
                }
            }
            finally {
                (routine as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// BootManager の static instance を設定
        /// </summary>
        private static void SetBootManagerInstance(BootManager instance) {
            var field = typeof(BootManager).GetField("s_instance", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(null, instance);
        }

        /// <summary>
        /// MainSystemStarter.Current を設定
        /// </summary>
        private static void SetStarterCurrent(MainSystemStarter starter) {
            var field = typeof(MainSystemStarter).GetField("<Current>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(null, starter);
        }

        /// <summary>
        /// private フィールドを設定
        /// </summary>
        private static void SetPrivateField<T>(object target, string fieldName, T value) {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        /// <summary>
        /// Object があれば即時破棄
        /// </summary>
        private static void DestroyImmediateIfExists(Object target) {
            if (target != null) {
                Object.DestroyImmediate(target);
            }
        }

    }
}
