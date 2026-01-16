using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.ActorSystems;
using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using R3;
using SampleGame.Application.ModelViewer;
using SampleGame.Domain;
using SampleGame.Domain.ModelViewer;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.ModelViewer;
using SampleGame.Presentation.ModelViewer;
using ThirdPersonEngine;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// モデルビューア用のSessionNode
    /// </summary>
    public class ModelViewerSessionNode : SceneSessionNode {
        private ModelViewerConfigData _configData;
        private ModelViewerAppService _appService;
        private ModelViewerDomainService _domainService;
        
        [Inject]
        private AssetManager _assetManager;

        /// <inheritdoc/>
        protected override string ScenePath => "Assets/SampleGame/Scenes/Develop/model_viewer.unity";

        /// <inheritdoc/>
        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);

            SetupInfrastructures(builder);
            SetupManagers(builder);
            SetupFactories(builder);
            SetupDomains(builder);
            SetupApplications(builder);
        }

        /// <inheritdoc/>
        protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.LoadRoutine(handle, scope);

            async UniTask LoadAsync(CancellationToken ct) {
                // Config読み込み
                await new ModelViewerConfigDataRequest()
                    .LoadAsync(_assetManager, scope, cancellationToken: ct)
                    .ContinueWith(x => {
                        _configData = x;
                        //ServiceContainer.RegisterInstance(x).RegisterTo(scope);
                    });
            }

            yield return LoadAsync(scope.Token).ToCoroutine();
        }

        /// <inheritdoc/>
        protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.InitializeRoutine(handle, scope);

            // アプリケーション初期化
            yield return _appService.SetupAsync(scope.Token).ToCoroutine();

            // カメラ操作用Controllerの設定
            var cameraManager = ObjectResolver.Resolve<CameraManager>();
            cameraManager.SetCameraHandler("Default", new PreviewCameraHandler(_configData.camera));

            // Recorderのセットアップ
            var recorder = ObjectResolver.Resolve<ModelRecorder>();
            recorder.ActorSlot = ObjectResolver.Resolve<ActorEntityManager>().RootTransform;

            // プレゼンテーション初期化
            SetupPresentations(scope);
        }

        /// <summary>
        /// Infrastructure層の初期化
        /// </summary>
        private void SetupInfrastructures(IContainerBuilder builder) {
            builder.Register<IModelRepository, ModelRepository>(Lifetime.Singleton);
            builder.Register<IModelViewerTableRepository, ModelViewerTableRepository>(Lifetime.Singleton);
            builder.Register<ModelViewerAssetRepository>(Lifetime.Singleton);
            builder.Register<EnvironmentSceneRepository>(Lifetime.Singleton);
        }

        /// <summary>
        /// Managerの初期化
        /// </summary>
        private void SetupManagers(IContainerBuilder builder) {
            var actorManager = new ActorEntityManager();
            builder.RegisterInstance(actorManager);

            // var cameraManager = ServiceResolver.Resolve<CameraManager>();
            // cameraManager.RegisterTask(TaskOrder.Camera);
        }

        /// <summary>
        /// Domain層の初期化
        /// </summary>
        private void SetupDomains(IContainerBuilder builder) {
            builder.Register<ModelViewerDomainService>(Lifetime.Singleton);
        }

        /// <summary>
        /// Application層の初期化
        /// </summary>
        private void SetupApplications(IContainerBuilder builder) {
            builder.Register<ModelViewerAppService>(Lifetime.Singleton);
        }

        /// <summary>
        /// Factoryの初期化
        /// </summary>
        private void SetupFactories(IContainerBuilder builder) {
            builder.Register<IPreviewActorFactory, PreviewActorFactory>(Lifetime.Singleton);
            builder.Register<IEnvironmentActorFactory, EnvironmentActorFactory>(Lifetime.Singleton);
        }

        /// <summary>
        /// Presentation層の初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            LogicUtility.CreateLogic<ModelViewerPresenter>(UpdateOrder.Presenter, ObjectResolver, true, scope);
        }
    }
}