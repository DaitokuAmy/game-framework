using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameFramework.AssetSystem;
using GameFramework.UISystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GameFramework.Tests {
    /// <summary>
    /// UISystem の回帰テスト
    /// </summary>
    public class UISystemTests {
        private UIManager _uiManager;
        private UpdateScheduler _updateScheduler;

        /// <summary>
        /// テスト前初期化
        /// </summary>
        [SetUp]
        public void SetUp() {
            _updateScheduler = new UpdateScheduler();
        }

        /// <summary>
        /// テスト後片付け
        /// </summary>
        [TearDown]
        public void TearDown() {
            _uiManager?.Dispose();
            _uiManager = null;

            _updateScheduler?.Dispose();
            _updateScheduler = null;

            DestroyImmediateIfExists(GameObject.Find("UIManager_Root"));
        }

        /// <summary>
        /// Prefab 読み込み失敗でも Handle が完了し、破棄後に再試行できることを検証
        /// </summary>
        [Test]
        public void LoadPrefabAsync_FailedHandle_ShouldCompleteAndAllowRetry() {
            var loader = new TestUIAssetLoader();
            loader.EnqueuePrefabInfo(new TestPrefabProcess(isDone: true, asset: null, exception: new Exception("first failure")));
            loader.EnqueuePrefabInfo(new TestPrefabProcess(isDone: true, asset: null, exception: new Exception("second failure")));

            _uiManager = new UIManager();
            _uiManager.Initialize(loader);
            _updateScheduler.RegisterUpdatable(_uiManager, 0);

            var firstHandle = _uiManager.LoadPrefabAsync("dialog");
            _updateScheduler.Update();

            Assert.That(firstHandle.IsDone, Is.True);
            Assert.That(firstHandle.Exception, Is.Not.Null);

            firstHandle.Dispose();

            var secondHandle = _uiManager.LoadPrefabAsync("dialog");

            Assert.That(loader.PrefabLoadCount, Is.EqualTo(2));
            Assert.That(secondHandle.Exception, Is.Not.Null);
        }

        /// <summary>
        /// Prefab 解放時に生成インスタンスも破棄されることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator LoadPrefabAsync_Dispose_ShouldDestroyInstantiatedPrefab() {
            var prefab = new GameObject("UiPrefab", typeof(RectTransform), typeof(Canvas));
            try {
                var loader = new TestUIAssetLoader();
                loader.EnqueuePrefabInfo(new TestPrefabProcess(isDone: true, asset: prefab));

                _uiManager = new UIManager();
                _uiManager.Initialize(loader);
                _updateScheduler.RegisterUpdatable(_uiManager, 0);

                var handle = _uiManager.LoadPrefabAsync("dialog");
                _updateScheduler.Update();

                var rootObject = GameObject.Find("UIManager_Root");
                Assert.That(rootObject, Is.Not.Null);
                Assert.That(rootObject.transform.childCount, Is.EqualTo(1));

                handle.Dispose();
                yield return null;

                Assert.That(rootObject.transform.childCount, Is.EqualTo(0));
            }
            finally {
                DestroyImmediateIfExists(prefab);
            }
        }

        /// <summary>
        /// Handler 解除時に OnUnregistered が呼ばれることを検証
        /// </summary>
        [Test]
        public void UnregisterHandler_ShouldInvokeOnUnregistered() {
            var root = CreateUiGameObject("ScreenRoot");
            try {
                var service = root.AddComponent<TestUIService>();
                var screen = root.AddComponent<TestScreen>();
                SetOpenOnStart(screen, false);

                ((IUIService)service).Initialize();
                ((IUIService)service).Update(0.0f);

                var handler = new TrackingHandler();
                screen.RegisterHandler(handler);
                screen.UnregisterHandler(handler);

                Assert.That(handler.RegisteredCount, Is.EqualTo(1));
                Assert.That(handler.UnregisteredCount, Is.EqualTo(1));
            }
            finally {
                DestroyImmediateIfExists(root);
            }
        }

        /// <summary>
        /// ResetHandler 後のダイアログ生成で Handler が再作成されないことを検証
        /// </summary>
        [Test]
        public void ResetHandler_ShouldPreventFutureHandlerCreation() {
            var root = CreateUiGameObject("DialogRoot");
            try {
                var container = root.AddComponent<UIDialogContainer>();
                var handlerFactories =
                    (Dictionary<string, Func<IUIScreenHandler>>)GetPrivateField(container, "_createHandlerFunctions");
                handlerFactories[DialogKey] = () => new TrackingHandler();

                container.ResetHandler(DialogKey);

                Assert.That(handlerFactories.ContainsKey(DialogKey), Is.False);
            }
            finally {
                DestroyImmediateIfExists(root);
            }
        }

        /// <summary>
        /// DialogInfo 破棄時にイベント購読が解除されることを検証
        /// </summary>
        [Test]
        public void DisposeDialogContainer_ShouldUnsubscribeDialogInfo() {
            var dialog = new TestStandaloneDialog();
            var dialogInfoType = typeof(UIDialogContainer).GetNestedType("DialogInfo", BindingFlags.NonPublic);
            Assert.That(dialogInfoType, Is.Not.Null);

            var dialogInfo = Activator.CreateInstance(dialogInfoType, new object[] { null, dialog, true });
            Assert.That(dialogInfo, Is.Not.Null);
            Assert.That(dialog.SubscriberCount, Is.EqualTo(1));

            ((IDisposable)dialogInfo).Dispose();

            Assert.That(dialog.SubscriberCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Skip でも完了通知が飛ぶことを検証
        /// </summary>
        [Test]
        public void Skip_ShouldInvokeFinishedEvent() {
            var player = new UIAnimationPlayer();
            try {
                var animation = new TestAnimation(0.5f);
                var handle = player.Play(animation);
                var finished = false;
                handle.FinishedEvent += () => finished = true;

                handle.Skip();

                Assert.That(finished, Is.True);
                Assert.That(handle.IsFinished, Is.True);
            }
            finally {
                player.Dispose();
            }
        }

        /// <summary>
        /// Remove で子画面の GameObject ごと破棄されることを検証
        /// </summary>
        [UnityTest]
        public IEnumerator Remove_ShouldDestroyChildScreenGameObject() {
            var root = CreateUiGameObject("ContainerRoot");
            var childObject = CreateUiGameObject("ChildScreen");
            try {
                var service = root.AddComponent<TestUIService>();
                var container = root.AddComponent<UIScreenContainer>();
                SetOpenOnStart(container, false);

                childObject.transform.SetParent(root.transform, false);
                var childScreen = childObject.AddComponent<TestScreen>();
                SetOpenOnStart(childScreen, false);

                ((IUIService)service).Initialize();
                ((IUIService)service).Update(0.0f);

                container.Add("child", childScreen);
                Assert.That(container.Remove("child"), Is.True);

                yield return null;

                Assert.That(childScreen == null, Is.True);
                Assert.That(childObject == null, Is.True);
            }
            finally {
                DestroyImmediateIfExists(root);
                DestroyImmediateIfExists(childObject);
            }
        }

        private const string DialogKey = "dialog";

        /// <summary>
        /// テスト用 UIService
        /// </summary>
        private sealed class TestUIService : UIService {
        }

        /// <summary>
        /// テスト用 UIScreen
        /// </summary>
        private sealed class TestScreen : UIScreen {
        }

        /// <summary>
        /// テスト用単体ダイアログ
        /// </summary>
        private sealed class TestStandaloneDialog : IDialog {
            private Action<int> _selectedIndexEvent;

            public int SubscriberCount { get; private set; }

            public event Action<int> SelectedIndexEvent {
                add {
                    SubscriberCount++;
                    _selectedIndexEvent += value;
                }
                remove {
                    SubscriberCount--;
                    _selectedIndexEvent -= value;
                }
            }

            public void Cancel() {
                _selectedIndexEvent?.Invoke(-1);
            }
        }

        /// <summary>
        /// テスト用 Handler
        /// </summary>
        private sealed class TrackingHandler : IUIScreenHandler {
            public int RegisteredCount { get; private set; }
            public int UnregisteredCount { get; private set; }

            public bool IsActive { get; private set; }

            public void OnRegistered(UIScreen screen) {
                RegisteredCount++;
            }

            public void OnUnregistered() {
                UnregisteredCount++;
            }

            public void PreOpen() {
            }

            public void PostOpen() {
            }

            public void Activate() {
                IsActive = true;
            }

            public void Update(float deltaTime) {
            }

            public void LateUpdate(float deltaTime) {
            }

            public void Deactivate() {
                IsActive = false;
            }

            public void PreClose() {
            }

            public void PostClose() {
            }
        }

        /// <summary>
        /// テスト用 UIAnimation
        /// </summary>
        private sealed class TestAnimation : IUIAnimation {
            public float Duration { get; }

            public TestAnimation(float duration) {
                Duration = duration;
            }

            public void SetTime(float time) {
            }

            public void OnPlay() {
            }
        }

        /// <summary>
        /// テスト用 UIAssetLoader
        /// </summary>
        private sealed class TestUIAssetLoader : IUIAssetLoader {
            private readonly Queue<TestPrefabProcess> _prefabProcesses = new();

            public int PrefabLoadCount { get; private set; }

            public void EnqueuePrefabInfo(TestPrefabProcess process) {
                _prefabProcesses.Enqueue(process);
            }

            public ISceneProcess LoadSceneAsync(string key) {
                return new TestSceneProcess();
            }

            public IProcess<GameObject> LoadPrefabAsync(string key) {
                PrefabLoadCount++;
                return _prefabProcesses.Dequeue();
            }

            public void UnloadScene(string key) {
            }

            public void UnloadPrefab(string key) {
            }
        }

        /// <summary>
        /// テスト用 PrefabProcess
        /// </summary>
        private sealed class TestPrefabProcess : IProcess<GameObject> {
            public bool IsDone { get; set; }
            public GameObject Result { get; }
            public Exception Exception { get; }
            public object Current => null;

            public TestPrefabProcess(bool isDone, GameObject asset, Exception exception = null) {
                IsDone = isDone;
                Result = asset;
                Exception = exception;
            }

            public bool MoveNext() {
                return !IsDone;
            }

            public void Reset() {
            }
        }

        /// <summary>
        /// テスト用 SceneProcess
        /// </summary>
        private sealed class TestSceneProcess : ISceneProcess {
            public bool IsDone => true;
            public Exception Exception => null;
            public Scene Scene => default;
            public bool IsValid => true;
            public Scene Result => Scene;
            public object Current => null;

            public AsyncOperationHandle ActivateAsync() {
                return AsyncOperationHandle.CompletedHandle;
            }

            public bool MoveNext() {
                return false;
            }

            public void Reset() {
            }
        }

        /// <summary>
        /// UI 用 GameObject を生成
        /// </summary>
        private static GameObject CreateUiGameObject(string name) {
            return new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        }

        /// <summary>
        /// openOnStart を変更
        /// </summary>
        private static void SetOpenOnStart(UIScreen screen, bool value) {
            var field = typeof(UIScreen).GetField("_openOnStart", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(screen, value);
        }

        /// <summary>
        /// private フィールド取得
        /// </summary>
        private static object GetPrivateField(object target, string fieldName) {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"{fieldName} should exist.");
            return field.GetValue(target);
        }

        /// <summary>
        /// 存在する場合のみ即時破棄
        /// </summary>
        private static void DestroyImmediateIfExists(Object target) {
            if (target != null) {
                Object.DestroyImmediate(target);
            }
        }
    }
}
